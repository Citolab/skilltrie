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

public interface IGroupService
{
    Task<Group> CreateGroup(string name);
    Task<Group> GetGroup(int id);
    Task<GroupMember> AddMember(int groupId, int userId);
    Task RemoveMember(int groupId, int userId);
    Task<IEnumerable<Group>> GetGroups(int offset, int range);
    Task<Group> UpdateGroup(int id, string name);
    Task DeleteGroup(int id);
    Task<(double avg, string filter)> GetGroupProficiency(int groupId, int? topicId, List<int>? topicIds);

    Task<ICollection<double>> GetScore(int groupId, int totalWeeks);
}

public class GroupService(AppDbContext context, IGroupHandler handler) : IGroupService
{
    /// <summary>
    /// Create a new Group
    /// </summary>
    /// <param name="name">The name of the new Group</param>
    /// <returns>The created <see cref="Group"/></returns>
    /// <exception cref="Exception">Something went wrong while creating a new group</exception>
    public async Task<Group> CreateGroup(string name)
    {
        var validation = handler.ValidateGroupName(name);
        if (!validation.ok)
            throw new Exception(validation.error);

        var group = new Group { Name = name };
        context.Groups.Add(group);
        await context.SaveChangesAsync();

        return group;
    }

    /// <summary>
    /// Fetch a group by its Id.
    /// </summary>
    /// <param name="id">Id of the group to fetch</param>
    /// <returns>The group matching the given id</returns>
    /// <exception cref="Exception">No <see cref="Group"/> with that Id was found</exception>
    public async Task<Group> GetGroup(int id)
    {
        var group = await context.Groups
            .Include(g => g.Members)
            .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(g => g.Id == id);

        return group ?? throw new Exception($"Group {id} not found");
    }

    /// <summary>
    /// Add a new member to a group
    /// </summary>
    /// <param name="groupId">The Id of the group to add a member to</param>
    /// <param name="userId">The Id of the user to add to the group</param>
    /// <returns>The new group member</returns>
    /// <exception cref="Exception">Something went wrong while adding the user to the group</exception>
    public async Task<GroupMember> AddMember(int groupId, int userId)
    {
        var info = await (
            from _ in context.Users.Where(u => u.Id == userId).DefaultIfEmpty()
            from __ in context.Groups.Where(g => g.Id == groupId).DefaultIfEmpty()
            select new
            {
                UserExists = context.Users.Any(u => u.Id == userId),
                GroupExists = context.Groups.Any(g => g.Id == groupId),
                AlreadyMember = context.GroupMembers.Any(m => m.GroupId == groupId && m.UserId == userId)
            }
        ).FirstAsync();

        var validation = handler.ValidateAddMember(info.UserExists, info.GroupExists, info.AlreadyMember);
        if (!validation.ok)
            throw new Exception(validation.error);

        var member = new GroupMember { GroupId = groupId, UserId = userId };
        context.GroupMembers.Add(member);
        await context.SaveChangesAsync();

        return member;
    }

    /// <summary>
    /// Remove a member from a given group
    /// </summary>
    /// <param name="groupId">The Id of the group to remove the member from</param>
    /// <param name="userId">The Id of the user to remove from the group</param>
    /// <exception cref="Exception">Something went wrong while removing the member from the group/exception>
    public async Task RemoveMember(int groupId, int userId)
    {
        var member = await context.GroupMembers
            .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == userId);

        if (member == null)
            throw new Exception("User is not in group");

        context.GroupMembers.Remove(member);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Get Groups for the dashboard
    /// </summary>
    /// <param name="offset">Offset to start Id's by</param>
    /// <param name="range">The range of Id's to get</param>
    /// <returns>A list of the requested groups</returns>
    /// <exception cref="Exception">Something went wrong while fetching groups</exception>
    public async Task<IEnumerable<Group>> GetGroups(int offset, int range)
    {
        var groups = await context.Groups
            .Include(g => g.Members)
            .ThenInclude(m => m.User)
            .OrderBy(g => g.Id)
            .Skip(offset)
            .Take(range)
            .ToListAsync();

        if (groups.Count == 0)
            return [];
        return groups;
    }

    /// <summary>
    /// Update a given group name
    /// </summary>
    /// <param name="id">The Id of the group to change</param>
    /// <param name="name">The new name of the group</param>
    /// <returns>The <see cref="Group"/> that's name was changed</returns>
    /// <exception cref="Exception">Something went wrong while updating the group's name</exception>
    public async Task<Group> UpdateGroup(int id, string name)
    {
        var validation = handler.ValidateGroupName(name);
        if (!validation.ok)
            throw new Exception(validation.error);

        var group = await context.Groups.FindAsync(id)
            ?? throw new Exception($"Group {id} not found");

        group.Name = name;
        await context.SaveChangesAsync();

        return group;
    }

    /// <summary>
    /// Delete a <see cref="Group"/> from the database
    /// </summary>
    /// <param name="id">The Id of the group to remove</param>
    /// <exception cref="Exception">Something went wrong while removing the group/exception>
    public async Task DeleteGroup(int id)
    {
        var group = await context.Groups
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == id)
            ?? throw new Exception($"Group {id} not found");

        if (group.Members.Any())
            context.GroupMembers.RemoveRange(group.Members);

        context.Groups.Remove(group);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Get the average proficiency of the group
    /// </summary>
    /// <param name="groupId">The Id of the group</param>
    /// <param name="topicId">The Topic to check</param>
    /// <param name="topicIds">The List of topics to check</param>
    /// <returns>The average proficiency</returns>
    public async Task<(double avg, string filter)> GetGroupProficiency(
        int groupId,
        int? topicId,
        List<int>? topicIds)
    {
        var query =
            from up in context.UserScopeProgress
            join gm in context.GroupMembers on up.UserId equals gm.UserId
            where gm.GroupId == groupId
            select up;

        string filterLabel;

        if (topicId.HasValue)
        {
            query = query.Where(up => up.ScopeId == topicId.Value);
            filterLabel = topicId.Value.ToString();
        }
        else if (topicIds is { Count: > 0 })
        {
            query = query.Where(up => topicIds.Contains(up.ScopeId));
            filterLabel = string.Join(",", topicIds);
        }
        else
        {
            filterLabel = "All";
        }

        var avg = await query
            .Select(up => (double?)up.Proficiency)
            .AverageAsync() ?? 0;

        return (avg, filterLabel);
    }

    //older code PoC
    public async Task<ICollection<double>> GetScore(int groupId, int totalWeeks)
    {
        //Add one day because day starts at 00:00 when using DateTime.Today
        var today = DateTime.Today.ToUniversalTime().AddDays(1);
        var startDate = today.AddDays(-7 * totalWeeks);

        var query = await (
            from userAnswer in context.UserAnswers
            join levelResult in context.LevelResults on userAnswer.LevelResultId equals levelResult.Id
            join user in context.Users on levelResult.UserId equals user.Id
            join groupMember in context.GroupMembers on user.Id equals groupMember.UserId
            let creationTime = levelResult.CreatedAt
            where groupMember.GroupId == groupId
                  && startDate <= creationTime && creationTime < today
            //group all answers by week
            let weekIndex = (int)(levelResult.CreatedAt.AddDays(-1) - startDate).TotalDays / 7
            group userAnswer by weekIndex into answersGrouped
            select new
            {
                WeekIndex = answersGrouped.Key,
                Ratio = (double)answersGrouped.Count(answer => answer.Correct) / answersGrouped.Count()
            }
        ).ToListAsync();

        //If a week doesn't have any answers it won't show up as a row in the query.
        //So we create array with fixed length so those weeks show up as 0.
        double[] results = new double[totalWeeks];
        foreach (var week in query)
        {
            results[week.WeekIndex] = week.Ratio;
        }

        return results;
    }
}
