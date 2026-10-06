/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using Models;
using Sprache;

namespace DatabasePopulator;

internal class InputUpdater
{
    // general update function
    // calls all other update functions, update the database according to the imported data
    public async Task Update(AppDbContext context)
    {
        Program.Status("\nStarting updating...");

        UpdateFunction[] updateFunctions =
        {
            UpdateItemsWithoutScope,
            UpdateUserProficiencies,
            UpdateBadges,
            UpdateUserCharacters
        };

        foreach (UpdateFunction updateFunction in updateFunctions)
            await updateFunction(context);

        Program.Success("\nDatabase successfully updated\n");
    }

    // update functions, each function updates a certain aspect of the database that is not imported
    // all formatting constraints should be included in these functions
    private delegate Task UpdateFunction(AppDbContext context);

    // Remove all items which have no scope attached, all that remains are the items actually able to be used
    private async Task UpdateItemsWithoutScope(AppDbContext context)
    {
        Program.Status("\nUpdating items...");

        List<Item> items = context.Items.ToList();
        List<int> scopeIds = context.Scopes.Select(scope => scope.Id).ToList();

        foreach (Item item in items)
            if (context.ScopeItems.All(scopeItem => scopeItem.ItemId != item.Id || !scopeIds.Contains(scopeItem.ScopeId)))
                context.Items.Remove(item);

        await context.SaveChangesAsync();

        Program.Success($"Successfully updated items");
    }

    // Add a user proficiency for each scope for each user
    private async Task UpdateUserProficiencies(AppDbContext context)
    {
        Program.Status("\nUpdating user proficiencies...");

        // Temporary code to which ensures a UserTopicProgress entry exists for each topic for each user. Does not actually save the old entry data.
        // This part is also the bottleneck of the topic parsing speed
        List<User> users = context.Users.ToList();
        List<Scope> scopes = context.Scopes.ToList();

        foreach (User user in users)
        {
            Random random = new Random(user.Id);

            foreach (Scope scope in scopes)
                if (await context.UserScopeProgress.FindAsync(user.Id, scope.Id) == null)
                    context.UserScopeProgress.Add(new UserScopeProgress
                    {
                        UserId = user.Id,
                        ScopeId = scope.Id,
                        Proficiency = decimal.Round(random.Next(500, 1001) / 1000m, 3)
                    });
        }

        await context.SaveChangesAsync();

        Program.Success($"Successfully updated user proficiencies");
    }

    // Ensure all badge information is added to the database
    private async Task UpdateBadges(AppDbContext context)
    {
        Program.Status("\nUpdating badges...");

        var parameterizedBadgeEntries = context.Scopes
            .Where(scope => scope.Type == ScopeType.Topic)
            .Select(scope => new ParameterizedBadgeEntry
            {
                BadgeImage = "TopicMasteredBadge",
                ScopeId = scope.Id
            });

        context.ParameterizedBadgeEntries.AddRange(parameterizedBadgeEntries);

        await context.SaveChangesAsync();

        Program.Success($"Successfully updated badges");
    }

    // Add all characters for each user and set a selected character
    private async Task UpdateUserCharacters(AppDbContext context)
    {
        Program.Status("\nUpdating user characters...");

        Random random = new();

        // Give all the characters to every existing user (for now)
        foreach (User user in context.Users.ToList())
            if (!context.UserCharacters.Any(uc => uc.UserId == user.Id))
            {
                List<UserCharacter> userCharacters = new();

                foreach (Character character in context.Characters.ToList())
                    userCharacters.Add(new UserCharacter
                    {
                        UserId = user.Id,
                        CharacterId = character.Id
                    });

                userCharacters[random.Next(userCharacters.Count)].Selected = true; // Set a random character to selected
                context.AddRange(userCharacters);
            }

        await context.SaveChangesAsync();

        Program.Success($"Successfully updated user characters");
    }
}
