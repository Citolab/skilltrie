/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using Models;
using API.Handlers;

namespace APITests.Handlers;

public class CosmeticHandlerTest
{
    private readonly UserCosmeticHandler _handler = new();

    [Fact(DisplayName = "NewUserCos throws exception when user is null")]
    public void NewUserCos_UserNullException()
    {
        User user = null!;
        Cosmetic cos = new Cosmetic { Id = 10 };

        Assert.Throws<ArgumentNullException>(() => _handler.NewUserCos(user, cos));
    }

    [Fact(DisplayName = "NewUserCos throws exception when cosmetic is null")]
    public void NewUserCos_CosNullException()
    {
        User user = new User { Id = 1 };
        Cosmetic cos = null!;

        Assert.Throws<ArgumentNullException>(() => _handler.NewUserCos(user, cos));
    }

    [Fact(DisplayName = "NewUserCos gives user cosmetic on valid inputs")]
    public void NewUserCos_validInputsUserCosmetic()
    {
        User user = new User { Id = 1 };
        Cosmetic cos = new Cosmetic { Id = 10 };

        var result = _handler.NewUserCos(user, cos);

        Assert.NotNull(result);
        Assert.Equal(user.Id, result.UserId);
        Assert.Same(user, result.User);

        Assert.Equal(cos.Id, result.CosmeticId);
        Assert.Same(cos, result.Cosmetic);

        Assert.False(result.Equipped);

    }

    [Fact(DisplayName = "EquipCos equips user cosmetic on valid inputs")]
    public void  EquipCos_equipsUserCosmetic(){
        User user = new User { Id = 1 };
        Cosmetic cos = new Cosmetic { Id = 10 };
        ICollection<UserCosmetic> userCosmetics = new List<UserCosmetic>();

        UserCosmetic userCosmetic = new UserCosmetic { User = user, Cosmetic = cos, Equipped = false };


        _handler.EquipCosmetic(userCosmetic, userCosmetics, true);

        Assert.True(userCosmetic.Equipped);
    }

    [Fact(DisplayName = "EquipCos does not equip duplicates")]
    public void  EquipCos_doesntEquipDuplicates()
    {
        User user = new User { Id = 1 };
        Cosmetic cos = new Cosmetic { Id = 10 };
        Cosmetic cos1 = new Cosmetic { Id = 11 };

        ICollection<UserCosmetic> userCosmetics = new List<UserCosmetic>();

        UserCosmetic userCosmetic = new UserCosmetic{ User = user, Cosmetic = cos, Equipped = true };
        UserCosmetic userCosmetic1 = new UserCosmetic{ User = user, Cosmetic = cos1, Equipped = false };

        userCosmetics.Add(userCosmetic);
        userCosmetics.Add(userCosmetic);

        _handler.EquipCosmetic(userCosmetic1, userCosmetics, true);

        Assert.False(userCosmetic.Equipped);
        Assert.True(userCosmetic1.Equipped);
    }

    [Fact(DisplayName = "PrepForDel unequips")]
    public void PrepForDelUnequips()
    {
        User user = new User { Id = 1 };
        Cosmetic cos = new Cosmetic { Id = 10 };

        UserCosmetic userCos = new  UserCosmetic { User = user, Cosmetic = cos, Equipped = true };

        _handler.PrepareForDeletion(userCos);

        Assert.False(userCos.Equipped);
    }
}
