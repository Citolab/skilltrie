/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using Microsoft.EntityFrameworkCore;
using Models;

namespace AA.UserProficiencyAlgorithm;

/// <summary>
/// The simplest implementation of the user proficiency algorithm
/// </summary>
/// <remarks>
/// Currently, this does not consider the outcome of the answer, so correct and incorrect both increment!
/// </remarks>
/// <param name="context"></param>
[Algorithm("Simple UPA")]
public class simpleUPA(AppDbContext context) : IUserProficiencyAlgorithm
{
    private readonly decimal _proficiencyGain = (decimal)0.1;
    /// <summary>
    /// Calculate new proficiency by adding 0.1 to the 
    /// </summary>
    /// <param name="proficiency"></param>
    /// <returns></returns>
    public decimal CalculateNewProficiency(decimal proficiency)
    {
        return Math.Clamp(proficiency + _proficiencyGain, 0, 1);
    }

    /// <summary>
    /// Updates the <see cref="UserScopeProgress"/> of one <see cref="User"/> for one <see cref="Scope"/>.
    /// </summary>
    /// <param name="userId">The Id of the user.</param>
    /// <param name="topicId">The Id of the topic.</param>
    public async Task UpdateProficiency(int userId, int topicId)
    {
        if (!context.Scopes.Any(t => t.Id == topicId))
            throw new Exception($"Topic not found");

        var prof = await (
            from up in context.UserScopeProgress
            where up.UserId == userId && up.ScopeId == topicId
            select up
        ).SingleAsync();

        prof.Proficiency = CalculateNewProficiency(prof.Proficiency);

        context.UserScopeProgress.Update(prof);
        await context.SaveChangesAsync();
    }
}
