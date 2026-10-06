/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using API.Handlers;
using Models;
using Microsoft.EntityFrameworkCore;

namespace API.Services;

public interface ICosmeticService
{
    Task<Cosmetic> GetCosmetic(int cosmeticId);
    Task<ICollection<Cosmetic>> GetCosmetics(ClothingType? type = null);
    Task<ICollection<Cosmetic>> GetDefaultCosmetics();
}
public class CosmeticService(AppDbContext context) : ICosmeticService
{
    /// <summary>
    /// Get a cosmetic based on it's identifier
    /// </summary>
    /// <param name="cosmeticId"> The identifier of the cosmetic.</param>
    /// <returns>
    /// On success, the task result contains the <see cref="Cosmetic"/>.
    /// Throws an exception if the cosmetic cannot be found.
    /// </returns>
    public async Task<Cosmetic> GetCosmetic(int cosmeticId)
    {
        var cos = await context.Cosmetics.FindAsync(cosmeticId)
            ?? throw new Exception("Cosmetic not found");
        return cos;
    }
    /// <summary>
    /// Gets a collection of cosmetics based on their <see cref="ClothingType"/>
    /// </summary>
    /// <param name="type"> The <see cref="ClothingType"/> of the requested cosmetics.</param>
    /// <returns>
    /// On success, the task result contains a collection of <see cref="Cosmetic"/>.
    /// Returns an empty collection, if no cosmetics can be found.
    /// </returns>
    public async Task<ICollection<Cosmetic>> GetCosmetics(ClothingType? type = null)
    {
        var query = context.Cosmetics.AsQueryable();

        if (type.HasValue)
            query = query.Where(c => c.ClothingType == type);

        return await query.ToListAsync();
    }

    public async Task<ICollection<Cosmetic>> GetDefaultCosmetics()
        => await context.Cosmetics.Where(c => c.DefaultOwned).ToListAsync();

}

public interface IUserCosmeticService
{
    Task<UserCosmetic> GetUserCosmetic(int userId, int cosId);
    Task<ICollection<UserCosmetic>> GetUserCosmetics(int userId, bool? equipped = null, ClothingType? type = null);
    Task<UserCosmetic> BuyUserCosmetic(int userId, int cosId);
    Task EquipUserCosmetic(int userId, int cosId, bool equipped);
    Task DeleteUserCosmetic(int userId, int cosId);
    Task SellUserCosmetic(int userId, int cosId);

}
public class UserCosmeticService(
    AppDbContext context,
    IUserService userService,
    ICosmeticService cosmeticService,

    IUserCurrencyService userCurrencyService,
    UserCosmeticHandler userCosmeticHandler
    ) : IUserCosmeticService
{
    /// <summary>
    /// Gets a cosmetic of a user
    /// </summary>
    /// <param name="userId"> The identifier of the user</param>
    /// <param name="cosId"> Optional. The identifier of the cosmetic</param>
    /// <returns>
    /// On success, the task result contains a collection of <see cref="UserCosmetic"/>.
    /// Throws an error if the user does not exist.
    /// </returns>
    public async Task<UserCosmetic> GetUserCosmetic(int userId, int cosId)
    {
        UserCosmetic userCosmetic = await context.UserCosmetics
            .Include(uc => uc.Cosmetic)
            .FirstOrDefaultAsync(uc =>
                uc.UserId == userId &&
                uc.CosmeticId == cosId)
            ?? throw new Exception("User does not have this cosmetic");

        return userCosmetic;
    }
    /// <summary>
    /// Gets a collection of the cosmetics of a user.
    /// </summary>
    /// <param name="userId"> The identifier of the user</param>
    /// <param name="equipped"> Optional. Filters the returned collection on equipped or unequipped items.</param>
    /// <param name="type"> Optional. Filters the returned collection on <see cref="ClothingType"/>.</param>
    /// <returns>
    /// On success, the task result contains a collection of <see cref="UserCosmetic"/>.
    /// Throws an error if the user does not exist.
    /// </returns>
    public async Task<ICollection<UserCosmetic>> GetUserCosmetics(int userId, bool? equipped = null, ClothingType? type = null)
    {
        await userService.VerifyUserExists(userId);

        var query = context.UserCosmetics
            .Where(c => c.UserId == userId);

        if (equipped.HasValue)
            query = query.Where(c => c.Equipped == equipped);

        if (type.HasValue)
            query = query.Where(c => c.Cosmetic.ClothingType == type);

        return await query.ToListAsync();
    }

    /// <summary>
    /// Buys a cosmetic for a User.
    /// </summary>
    /// <param name="userId"> The identifier of the user</param>
    /// <param name="cosId"> The identifier of the cosmetic.</param>
    /// <returns>
    /// On success, the task result contains a <see cref="UserCosmetic"/>.
    /// Throws an error if the user does not exist or already own the cosmetic.
    /// </returns>
    public async Task<UserCosmetic> BuyUserCosmetic(int userId, int cosId)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            User user = await userService.GetUser(userId);
            Cosmetic cos = await cosmeticService.GetCosmetic(cosId);

            bool alreadyOwned = await context.UserCosmetics
                .AnyAsync(uc => uc.UserId == userId && uc.CosmeticId == cosId);

            if (alreadyOwned)
                throw new Exception("User already owns cosmetic");

            await userCurrencyService.UpdateUserCurrency(userId, cos.CurrencyId, -cos.Price);
            UserCosmetic userCos = userCosmeticHandler.NewUserCos(user, cos);

            context.UserCosmetics.Add(userCos);
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            return userCos;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    /// <summary>
    /// Equip or unequip a cosmetic for a User.
    /// </summary>
    /// <param name="userId"> The identifier of the user</param>
    /// <param name="cosId"> The identifier of the cosmetic.</param>
    /// <param name="equipped"> Whether to equip or unequip a cosmetic</param>
    /// <returns>
    /// On success, an empty task result .
    /// Throws an error if the user does not exist or does not have the cosmetic.
    /// </returns>
    public async Task EquipUserCosmetic(int userId, int cosId, bool equipped)
    {
        var userCos = await GetUserCosmetic(userId, cosId);

        var clothingType = userCos.Cosmetic.ClothingType;

        var sameTypeCosmetics = await context.UserCosmetics
            .Include(uc => uc.Cosmetic)
            .Where(uc =>
                uc.UserId == userId &&
                uc.Cosmetic.ClothingType == clothingType &&
                uc.Equipped)
            .ToListAsync();

        userCosmeticHandler.EquipCosmetic(userCos, sameTypeCosmetics, equipped);

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Remove a cosmetic from the inventoru of a User.
    /// </summary>
    /// <param name="userId"> The identifier of the user</param>
    /// <param name="cosId"> The identifier of the cosmetic.</param>
    /// <returns>
    /// On success, an empty task result.
    /// Throws an error if the user does not have the cosmetic.
    /// </returns>
    public async Task DeleteUserCosmetic(int userId, int cosId)
    {
        var userCos = await GetUserCosmetic(userId, cosId);
        userCosmeticHandler.PrepareForDeletion(userCos);
        context.UserCosmetics.Remove(userCos);
        await context.SaveChangesAsync();
    }
    /// <summary>
    /// Sell a cosmetic from the inventory of a User.
    /// </summary>
    /// <param name="userId"> The identifier of the user</param>
    /// <param name="cosId"> The identifier of the cosmetic.</param>
    /// <returns>
    /// On success, an empty task result.
    /// Throws an error if the user does not have the cosmetic.
    /// </returns>
    public async Task SellUserCosmetic(int userId, int cosId)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            Cosmetic cos = await cosmeticService.GetCosmetic(cosId);
            await userCurrencyService.UpdateUserCurrency(userId, cos.CurrencyId, cos.Price);
            await DeleteUserCosmetic(userId, cosId);
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}


