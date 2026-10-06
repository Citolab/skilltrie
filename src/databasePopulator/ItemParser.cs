/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using Models;
using Sprache;
using System.Xml;
using System.Xml.Linq;

namespace DatabasePopulator;

internal partial class InputParser
{
    // Parse all item files listed in the designated file
    private async Task ParseItems(AppDbContext context)
    {
        Program.Status("\nImporting items...");

        string[] file = await File.ReadAllLinesAsync(Program.GetAbsolutePath(_inputDirectory, _itemsFile));
        List<Item> batch = new(_batchSize);
        int count = 0,
            skipped = 1;
        XmlReaderSettings xmlReaderSettings = new()
        {
            Async = true,
            IgnoreComments = true,
            IgnoreWhitespace = true
        };

        foreach (string line in file.Skip(skipped))
        {
            try
            {
                string[] data = line.Split(",");

                if (data.Length < 7)
                    throw new Exception($"Invalid line '{line}'");

                if (!int.TryParse(data[0], out int itemId))
                    throw new Exception($"Item.Id='{data[0]}' is not a valid Id");
                else if (context.Items.Any((Item item) => item.Id == itemId) || batch.Any(item => item.Id == itemId))
                    throw new Exception($"Item.Id='{itemId}' is a duplicate Id");

                string itemBodyHTML;
                ICollection<ItemAnswer> itemAnswers;
                string? itemFeedbackHTML;
                Item.ItemType itemType;
                string itemResponseType = data[3];
                Item.Language itemLanguage;
                int randomAppearanceCount;
                string itemDifficulty = data[5];

                using (var fileStream = File.OpenRead(Program.GetAbsolutePath(_itemsDirectory, data[6])))
                using (var xmlReader = XmlReader.Create(fileStream, xmlReaderSettings))
                {
                    XDocument xDocument = XDocument.Load(xmlReader) ?? throw new Exception($"Item.Id='{itemId}' has no root element");
                    XNamespace nameSpace = xDocument.Root!.Name.Namespace;

                    string? itemAnswerIdentifier = xDocument.Descendants(nameSpace + "qti-correct-response").FirstOrDefault()?.Value;

                    XElement itemBody = xDocument.Descendants(nameSpace + "qti-item-body").FirstOrDefault() ?? throw new Exception($"Item.Id='{itemId}' has no item body");
                    XElement itemBodyClone = new(itemBody);
                    itemBodyClone.Descendants(nameSpace + "qti-choice-interaction").Remove();
                    RemoveAttributes(itemBodyClone.DescendantsAndSelf());
                    itemBodyHTML = CleanHTML(string.Concat(itemBodyClone.Nodes().Select(n => n.ToString().Trim())));

                    var itemAnswerOptions = xDocument.Descendants(nameSpace + "qti-choice-interaction").Descendants(nameSpace + "qti-simple-choice")
                        .Select(xElement => (
                            answer: CleanHTML(string.Concat(xElement.Nodes().Select(node => node.ToString().Trim()))),
                            identifier: xElement.Attribute("identifier")?.Value ?? throw new Exception($"Item.Id='{itemId}' does not have identifiers for all answers")
                            )
                            );

                    var itemFeedback = xDocument.Descendants(nameSpace + "qti-modal-feedback").Descendants(nameSpace + "qti-content-body");
                    RemoveAttributes(itemFeedback.DescendantsAndSelf());
                    itemFeedbackHTML = CleanHTML(string.Concat(itemFeedback.Nodes().Select(node => node.ToString().Trim())));
                    if (itemFeedbackHTML.Length == 0)
                        itemFeedbackHTML = null;

                    string? responseDeclarationIdentifier = xDocument.Descendants(nameSpace + "qti-response-declaration")?.FirstOrDefault()?.Attribute("identifier")?.Value;

                    if (itemResponseType.Length <= 0 || itemResponseType.Length > 255)
                        throw new Exception($"Item.ResponseType='{itemResponseType}' is not a valid response type");

                    if (itemDifficulty.Length <= 0 || itemDifficulty.Length > 255)
                        throw new Exception($"Item.Level='{itemDifficulty}' is not a valid difficulty");

                    switch (data[2])
                    {
                        case "open_numeric":
                            itemType = Item.ItemType.Numerical;

                            if (itemAnswerIdentifier == null)
                                throw new Exception($"Item.Id='{itemId}' has no correct response for numerical item type");
                            else
                                itemAnswers = [new ItemAnswer() {
                                AnswerIdentifier = responseDeclarationIdentifier,
                                AnswerText = itemAnswerIdentifier,
                                Correct = true
                            }];

                            break;
                        case "open_text":
                            itemType = Item.ItemType.Open;

                            itemAnswers = [new ItemAnswer() {
                            AnswerIdentifier = responseDeclarationIdentifier,
                            AnswerText = itemAnswerIdentifier,
                            Correct = false
                        }];

                            break;
                        case "multiple_choice":
                            itemType = Item.ItemType.MultipleChoice;

                            itemAnswers = [..itemAnswerOptions.Select((tuple) => new ItemAnswer() {
                            AnswerIdentifier = tuple.identifier,
                            AnswerText = tuple.answer,
                            Correct = tuple.identifier == itemAnswerIdentifier
                        })];

                            break;
                        default:
                            throw new Exception($"Item.Id='{itemId}' has no valid item type");
                    }

                    randomAppearanceCount = AddRandomAppearanceCountToItems(itemAnswers);

                    itemLanguage = data[4] switch
                    {
                        "english" => Item.Language.English,
                        "dutch" => Item.Language.Dutch,
                        _ => throw new Exception($"Item.Id='{itemId}' has no valid language")
                    };
                }

                batch.Add(new Item
                {
                    Id = itemId,
                    Active = true,
                    QuestionText = itemBodyHTML,
                    Answers = itemAnswers,
                    Source = Item.ItemSource.Databank,
                    AnswerExplanation = itemFeedbackHTML,
                    Type = itemType,
                    ResponseType = itemResponseType,
                    Lang = itemLanguage,
                    AppearanceCount = randomAppearanceCount,
                    Level = itemDifficulty
                });

                count++;
            }
            catch (Exception exception)
            {
                Program.Error(exception);
            }

            if (batch.Count >= _batchSize)
            {
                context.Items.AddRange(batch);
                context.ItemUrns.AddRange(batch.Select(item => new ItemUrn
                {
                    ItemId = item.Id
                }));
                batch.Clear();
                await context.SaveChangesAsync();
            }
        }

        if (batch.Count > 0)
        {
            context.Items.AddRange(batch);
            context.ItemUrns.AddRange(batch.Select(item => new ItemUrn
            {
                ItemId = item.Id
            }));
            await context.SaveChangesAsync();
        }

        Program.Success($"Successfully imported items: {count}/{file.Length - skipped} lines parsed");
    }
}
