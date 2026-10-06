/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using CsvHelper;
using Models;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace DatabasePopulator;

internal partial class InputParser
{
    // the size of the batch used to determine when to save to the DB
    private const int _batchSize = 100;

    // location of the directories used to read files from
    private const string _inputDirectory = "input";
    private const string _itemsDirectory = "items";

    // names of the input files
    private const string _scopesFile = "scopes.csv";
    private const string _scopeMembershipFile = "scope_membership.csv";
    private const string _ScopeEdgelistFile = "scope_edgelist.csv";
    private const string _itemsFile = "items.csv";
    private const string _itemsMembershipFile = "items_membership.csv";
    private const string _charactersFile = "characters.csv";
    private const string _cosmeticsFile = "cosmetics.csv";

    // maps the given input for the type of a scope in _scopesFile to the actual ScopeType type
    private readonly Dictionary<string, ScopeType> _scopeTypeMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Subject"] = ScopeType.Subject,
        ["Topic"] = ScopeType.Topic,
        ["Domain"] = ScopeType.Domain
    };

    // regex used when parsing
    private static readonly Regex _removeExcessWhitespaceRegex = new(@"\s{2,}", RegexOptions.Compiled);
    private static readonly Regex _removeXmlnsAttributeRegex = new(@"(?<!<math)\sxmlns="".*?""", RegexOptions.Compiled);

    // parse functions, each function reads a specific file
    // all formatting constraints should be included in these functions
    private delegate Task ParserFunction(AppDbContext context);

    // general parse function
    // calls all other parse functions, stores the imported data
    public async Task Parse(AppDbContext context)
    {
        Program.Status("\nStarting importing...");

        ParserFunction[] parserFunctions =
        {
            ParseScopes,
            ParseScopeMemberships,
            ParseScopeEdgelist,
            ParseItems,
            ParseScopeItems,
            ParseCharacters,
            ParseCosmetics
        };

        foreach (ParserFunction parserFunction in parserFunctions)
            await parserFunction(context);

        Program.Success("\nFiles successfully imported\n");
    }

    // Parse all scopes from the designated file
    private async Task ParseScopes(AppDbContext context)
    {
        Program.Status("\nImporting scopes...");

        string[] file = await File.ReadAllLinesAsync(Program.GetAbsolutePath(_inputDirectory, _scopesFile));
        List<Scope> batch = new(_batchSize);
        int count = 0,
            skipped = 1;

        foreach (string line in file.Skip(skipped))
        {
            try
            {
                string[] data = line.Split(',');

                if (data.Length < 3)
                    throw new Exception($"Invalid line '{line}'");

                if (!int.TryParse(data[0], out int id))
                    throw new Exception($"Scope.Id='{data[0]}' is not a valid Id");
                else if (context.Scopes.Any((Scope scope) => scope.Id == id) || batch.Any(scope => scope.Id == id))
                    throw new Exception($"Scope.Id='{id}' is a duplicate Id");

                string name = data[1];

                if (!_scopeTypeMap.TryGetValue(data[2], out ScopeType scopeType))
                    throw new Exception($"Scope.Type='{data[2]}' is not a valid scope type");

                batch.Add(new Scope
                {
                    Id = id,
                    Name = name,
                    Type = scopeType
                });

                count++;
            }
            catch (Exception exception)
            {
                Program.Error(exception);
            }

            if (batch.Count >= _batchSize)
            {
                context.Scopes.AddRange(batch);
                batch.Clear();
                await context.SaveChangesAsync();
            }
        }

        if (batch.Count > 0)
        {
            context.Scopes.AddRange(batch);
            await context.SaveChangesAsync();
        }

        Program.Success($"Successfully imported scopes: {count}/{file.Length - skipped} lines parsed");
    }

    // Parse all scope memberships from the designated file
    private async Task ParseScopeMemberships(AppDbContext context)
    {
        Program.Status("\nImporting scope memberships...");

        string[] file = await File.ReadAllLinesAsync(Program.GetAbsolutePath(_inputDirectory, _scopeMembershipFile));
        List<ScopeMembership> batch = new(_batchSize);
        int count = 0,
            skipped = 1;

        foreach (string line in file.Skip(skipped))
        {
            try
            {
                string[] data = line.Split(",");

                if (data.Length < 2)
                    throw new Exception($"Invalid line '{line}'");

                if (!int.TryParse(data[0], out int ancestorId))
                    throw new Exception($"ScopeMembership.AncestorId='{data[0]}' is not a valid Id");
                else if (!context.Scopes.Any((Scope x) => x.Id == ancestorId))
                    throw new Exception($"ScopeMembership.AncestorId='{ancestorId}' does not exist");

                if (!int.TryParse(data[1], out int descendantId))
                    throw new Exception($"ScopeMembership.DescendantId='{data[1]}' is not a valid Id");
                else if (!context.Scopes.Any((Scope x) => x.Id == descendantId))
                    throw new Exception($"ScopeMembership.DescendantId='{descendantId}' does not exist");

                if (
                    context.ScopeMemberships.Any((ScopeMembership scopeMembership) => scopeMembership.AncestorId == ancestorId && scopeMembership.DescendantId == descendantId)
                    || batch.Any(scopeMembership => scopeMembership.AncestorId == ancestorId && scopeMembership.DescendantId == descendantId)
                    )
                    throw new Exception($"(ScopeMembership.AncestorId='{ancestorId}', ScopeMembership.DescendantId='{descendantId}') is a duplicate tuple");

                batch.Add(new ScopeMembership
                {
                    AncestorId = ancestorId,
                    DescendantId = descendantId
                });

                count++;
            }
            catch (Exception exception)
            {
                Program.Error(exception);
            }

            if (batch.Count >= _batchSize)
            {
                context.ScopeMemberships.AddRange(batch);
                batch.Clear();
                await context.SaveChangesAsync();
            }
        }

        if (batch.Count > 0)
        {
            context.ScopeMemberships.AddRange(batch);
            await context.SaveChangesAsync();
        }

        Program.Success($"Successfully imported scope memberships: {count}/{file.Length - skipped} lines parsed");
    }

    // Parse the edgelist from the designated file
    private async Task ParseScopeEdgelist(AppDbContext context)
    {
        Program.Status("\nImporting scope edgelist...");

        string[] file = await File.ReadAllLinesAsync(Program.GetAbsolutePath(_inputDirectory, _ScopeEdgelistFile));
        List<ScopeEdge> batch = new(_batchSize);
        int count = 0,
            skipped = 1;

        foreach (string line in file.Skip(skipped))
        {
            try
            {
                string[] data = line.Split(",");

                if (data.Length < 3)
                    throw new Exception($"Invalid line '{line}'");

                if (!int.TryParse(data[0], out int fromScopeId))
                    throw new Exception($"ScopeEdge.FromScopeId='{data[0]}' is not a valid Id");
                else if (!context.Scopes.Any((Scope x) => x.Id == fromScopeId))
                    throw new Exception($"ScopeEdge.FromScopeId='{fromScopeId}' does not exist");
                else if (context.Scopes.Find(fromScopeId)!.Type != ScopeType.Topic)
                    throw new Exception($"ScopeEdge.FromScopeId='{fromScopeId}' is not a topic");

                if (!int.TryParse(data[1], out int toScopeId))
                    throw new Exception($"ScopeEdge.ToScopeId='{data[1]}' is not a valid Id");
                else if (!context.Scopes.Any((Scope x) => x.Id == toScopeId))
                    throw new Exception($"ScopeEdge.ToScopeId='{toScopeId}' does not exist");
                else if (context.Scopes.Find(toScopeId)!.Type != ScopeType.Topic)
                    throw new Exception($"ScopeEdge.ToScopeId='{toScopeId}' is not a topic");

                if (!decimal.TryParse(data[2], out decimal weight))
                    throw new Exception($"ScopeEdge.FromScopeId='{data[2]}' is not a valid weight");
                else if (weight < 0 || weight > 1)
                    throw new Exception($"ScopeEdge.FromScopeId='{weight}' must be greater or equal to 0 or smaller or equal to 1");

                if (
                    context.ScopeEdges.Any((ScopeEdge scopeEdge) => scopeEdge.FromScopeId == fromScopeId && scopeEdge.ToScopeId == toScopeId)
                    || batch.Any(scopeEdge => scopeEdge.FromScopeId == fromScopeId && scopeEdge.ToScopeId == toScopeId)
                    )
                    throw new Exception($"(ScopeEdge.FromScopeId='{fromScopeId}', ScopeEdge.ToScopeId='{toScopeId}') is a duplicate tuple");

                batch.Add(new ScopeEdge
                {
                    FromScopeId = fromScopeId,
                    ToScopeId = toScopeId,
                    Weight = weight
                });

                count++;
            }
            catch (Exception exception)
            {
                Program.Error(exception);
            }

            if (batch.Count >= _batchSize)
            {
                context.ScopeEdges.AddRange(batch);
                batch.Clear();
                await context.SaveChangesAsync();
            }
        }

        if (batch.Count > 0)
        {
            context.ScopeEdges.AddRange(batch);
            await context.SaveChangesAsync();
        }

        Program.Success($"Successfully imported scope edgelist: {count}/{file.Length - skipped} lines parsed");
    }

    // Parse all item memberships from the designated file
    private async Task ParseScopeItems(AppDbContext context)
    {
        Program.Status("\nImporting item memberships...");

        string[] file = await File.ReadAllLinesAsync(Program.GetAbsolutePath(_inputDirectory, _itemsMembershipFile));
        List<ScopeItem> batch = new(_batchSize);
        int count = 0,
            skipped = 0;

        foreach (string line in file.Skip(skipped))
        {
            try
            {
                string[] data = line.Split(",");

                if (data.Length < 2)
                    throw new Exception($"Invalid line '{line}'");

                if (!int.TryParse(data[0], out int itemId))
                    throw new Exception($"ScopeItem.ItemId='{data[0]}' is not a valid Id");
                else if (!context.Items.Any((Item x) => x.Id == itemId))
                    throw new Exception($"ScopeItem.ItemId='{itemId}' does not exist");

                if (!int.TryParse(data[1], out int scopeId))
                    throw new Exception($"ScopeItem.ScopeId='{data[1]}' is not a valid Id");
                else if (!context.Scopes.Any((Scope x) => x.Id == scopeId))
                    throw new Exception($"ScopeItem.ScopeId='{scopeId}' does not exist");

                if (
                    context.ScopeItems.Any((ScopeItem scopeItem) => scopeItem.ItemId == itemId && scopeItem.ScopeId == scopeId)
                    || batch.Any(scopeItem => scopeItem.ItemId == itemId && scopeItem.ScopeId == scopeId)
                    )
                    throw new Exception($"(ScopeItem.ItemId='{itemId}', ScopeItem.ScopeId='{scopeId}') is a duplicate tuple");

                batch.Add(new ScopeItem
                {
                    ItemId = itemId,
                    ScopeId = scopeId
                });

                count++;
            }
            catch (Exception exception)
            {
                Program.Error(exception);
            }

            if (batch.Count >= _batchSize)
            {
                context.ScopeItems.AddRange(batch);
                batch.Clear();
                await context.SaveChangesAsync();
            }
        }

        if (batch.Count > 0)
        {
            context.ScopeItems.AddRange(batch);
            await context.SaveChangesAsync();
        }

        Program.Success($"Successfully imported item memberships: {count}/{file.Length - skipped} lines parsed");
    }

    // Parse all characters from the designated file
    private async Task ParseCharacters(AppDbContext context)
    {
        Program.Status("\nImporting characters...");

        using (StreamReader streamReader = new(Program.GetAbsolutePath(_inputDirectory, _charactersFile)))
        using (CsvReader csvReader = new(streamReader, CultureInfo.InvariantCulture))
        {
            csvReader.Context.RegisterClassMap<CharacterMap>();
            List<Character> records = csvReader.GetRecords<Character>().ToList();
            context.Characters.AddRange(records);
            await context.SaveChangesAsync();
        }

        Program.Success("Successfully imported characters");
    }

    // Parse all cosmetics from the designated file
    private async Task ParseCosmetics(AppDbContext context)
    {
        Program.Status("\nImporting cosmetics...");

        using (StreamReader streamReader = new(Program.GetAbsolutePath(_inputDirectory, _cosmeticsFile)))
        using (CsvReader csvReader = new(streamReader, CultureInfo.InvariantCulture))
        {
            csvReader.Context.RegisterClassMap<CosmeticMap>();
            List<Cosmetic> records = csvReader.GetRecords<Cosmetic>().ToList();
            context.Cosmetics.AddRange(records);
            await context.SaveChangesAsync();
        }

        Program.Success("Successfully imported cosmetics");
    }

    // For development purposes, we randomly add an amount of
    // appearances (how many times an item has been picked for a test).
    // For deployment, this should be turned off (aka set to 0).
    private static int AddRandomAppearanceCountToItems(ICollection<ItemAnswer> itemAnswers)
    {
        Random random = new();
        int randomAppearanceCount = random.Next(10, 1234);
        List<int> cuts = new([0, randomAppearanceCount]);
        List<int> parts = new(itemAnswers.Count);

        for (int i = 0; i < itemAnswers.Count - 1; i++)
            cuts.Add(random.Next(0, randomAppearanceCount + 1));

        cuts.Sort();

        for (int i = 1; i < cuts.Count; i++)
            parts.Add(cuts[i] - cuts[i - 1]);

        int n = 0;

        foreach (ItemAnswer itemAnswer in itemAnswers)
        {
            itemAnswer.Chosen = parts[n];
            n++;
        }

        return randomAppearanceCount;
    }

    // Helper function to read item files
    internal static void RemoveAttributes(IEnumerable<XElement> elements)
    {
        // preserve attributes on <math> and <img> elements
        foreach (XElement xElement in elements)
            if (
                xElement.Name.LocalName != "img"
                && xElement.Name.LocalName != "math"
                && xElement.Name.LocalName != "qti-text-entry-interaction"
                && xElement.Name.LocalName != "qti-extended-text-interaction"
                && xElement.Name.LocalName != "qti-choice-interaction"
            )
                xElement.RemoveAttributes();
    }

    // regex horrors to remove excess whitespaces and xlmns attributes because XDocument won't budge
    internal static string CleanHTML(string html)
    {
        return _removeXmlnsAttributeRegex.Replace(
            _removeExcessWhitespaceRegex.Replace(
                html,
                " "
                ),
            ""
            );
    }
}
