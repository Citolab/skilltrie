/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using System.Text.RegularExpressions;
using Models;

namespace API.Tools.AI;

/// <summary>
/// Represents a key/value token extracted from text metadata.
/// </summary>
/// <param name="Key">The metadata key.</param>
/// <param name="Value">The metadata value.</param>
public record Token(string Key, string Value);

/// <summary>
/// makes tokens for the item parser to make an item
/// tokenizes the AI propmt
/// </summary>
public class ItemTokenizer
{
    /// <summary>
    /// Extracts metadata tokens from the given text.
    /// </summary>
    /// <param name="text">The raw text containing metadata lines.</param>
    /// <returns>An enumerable of <see cref="Token"/> objects with key/value pairs.</returns>
    public IEnumerable<Token> ExtractMetadata(string text)
    {
        var metaPattern = @"(?m)^(?<key>[A-Za-z]+)\s+(?<value>.+)$";
        var matches = Regex.Matches(text, metaPattern);

        foreach (Match m in matches)
        {
            yield return new Token(
                m.Groups["key"].Value.Trim(),
                m.Groups["value"].Value.Trim()
            );
        }
    }

    /// <summary>
    /// Extracts a section of text between a start marker and an optional end marker.
    /// </summary>
    /// <param name="text">The raw text to search.</param>
    /// <param name="start">The start marker string.</param>
    /// <param name="end">Optional end marker string. If null, extracts until the end of the text.</param>
    /// <returns>The extracted section content, or an empty string if not found.</returns>
    public string ExtractSection(string text, string start, string? end = null)
    {
        var pattern = end == null
            ? @$"{start}\s*(?<content>[\s\S]*)"
            : @$"{start}\s*(?<content>[\s\S]*?)(?=\b{end}\b)";

        var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
        if (!match.Success)
        {
            return string.Empty;
        }

        var content = match.Groups["content"].Value.Trim();
        return content;
    }

    /// <summary>
    /// Extracts multiple choice options from the given text.
    /// </summary>
    /// <param name="text">The raw text containing option lines (A.–D.).</param>
    /// <returns>An enumerable of tuples with option ID (A–D) and answer text.</returns>
    public IEnumerable<(string Id, string Answer)> ExtractOptions(string text)
    {
        var optionPattern = @"(?m)^(?<id>[A-D])\.\s+(?<answer>.+)$";
        var matches = Regex.Matches(text, optionPattern);

        foreach (Match m in matches)
        {
            yield return (
                m.Groups["id"].Value.Trim(),
                m.Groups["answer"].Value.Trim()
            );
        }
    }
}

/// <summary>
/// Parses the tokens from the itemTokenizer and makes an item from it
/// </summary>
public class ItemParser
{
    private readonly ItemTokenizer _tokenizer;

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemParser"/> class.
    /// </summary>
    /// <param name="tokenizer">Tokenizer used to extract metadata and sections.</param>
    public ItemParser(ItemTokenizer tokenizer)
    {
        _tokenizer = tokenizer;
    }

    /// <summary>
    /// Parses an <see cref="Item"/> object from raw text.
    /// </summary>
    /// <param name="text">The raw text containing metadata, question, options, and solution.</param>
    /// <returns>An <see cref="Item"/> populated with parsed fields and answers.</returns>
    public Item ParseFromText(string text)
    {
        var item = new Item { Active = true, Source = Item.ItemSource.LLM, AppearanceCount = 0 };

        foreach (var token in _tokenizer.ExtractMetadata(text))
        {
            MapMetadata(item, token);
        }

        item.QuestionText = _tokenizer.ExtractSection(text, "Question", "Solution");
        ParseAnswers(text, item);

        EnsureDefaults(item, text);

        return item;
    }

    /// <summary>
    /// Maps a metadata token to the corresponding property on the <see cref="Item"/>.
    /// </summary>
    /// <param name="item">The item to update.</param>
    /// <param name="token">The metadata token containing key and value.</param>
    private void MapMetadata(Item item, Token token)
    {
        switch (token.Key.ToLower())
        {
            case "language":
                item.Lang = token.Value.Equals("english", StringComparison.OrdinalIgnoreCase)
                    ? Item.Language.English
                    : Item.Language.Dutch;
                break;
            case "level":
                item.Level = token.Value;
                break;
            case "responsetype":
                item.ResponseType = token.Value;
                break;
            case "type":
                item.Type = token.Value.Equals("num", StringComparison.OrdinalIgnoreCase)
                    ? Item.ItemType.Numerical
                    : token.Value.Contains("open", StringComparison.OrdinalIgnoreCase)
                        ? Item.ItemType.Open
                        : Item.ItemType.MultipleChoice;
                break;
            case "solution":
                item.AnswerExplanation = token.Value;
                break;
        }
    }

    /// <summary>
    /// Parses answers from the text and populates the <see cref="Item.Answers"/> collection.
    /// Handles both multiple choice and open/numerical item types.
    /// </summary>
    /// <param name="text">The raw text containing options and/or solution.</param>
    /// <param name="item">The item to update with parsed answers.</param>
    private void ParseAnswers(string text, Item item)
    {
        if (item.Type == Item.ItemType.MultipleChoice)
        {
            var options = _tokenizer.ExtractOptions(
                _tokenizer.ExtractSection(text, "Options", "Solution")
            );

            int counter = 1;
            foreach (var (id, answerText) in options)
            {
                item.Answers.Add(new ItemAnswer
                {
                    AnswerText = answerText,
                    AnswerIdentifier = $"qti_21_items_all_238788197_section_0000_item_1_schoice_{counter}_{DateTime.UtcNow.Ticks}",
                    Correct = false,
                    Chosen = 0
                });
                counter++;
            }

            var correctPattern = @"Correct\..*?([A-D])";
            var correctMatch = Regex.Match(text, correctPattern, RegexOptions.IgnoreCase);

            if (correctMatch.Success)
            {
                string correctLetter = correctMatch.Groups[1].Value.ToUpper();

                foreach (var a in item.Answers)
                {
                    var plain = Regex.Replace(a.AnswerText ?? "", "<.*?>", "").Trim();

                    if (plain.StartsWith(correctLetter, StringComparison.OrdinalIgnoreCase) ||
                        plain.StartsWith($"{correctLetter}.", StringComparison.OrdinalIgnoreCase))
                    {
                        a.Correct = true;
                    }
                }
            }
        }
        else
        {
            var solution = _tokenizer.ExtractSection(text, "Solution");
            item.Answers.Add(new ItemAnswer
            {
                AnswerText = solution,
                AnswerIdentifier = $"qti_21_items_all_238788197_section_0000_item_1_num_RESPONSE_1_{DateTime.UtcNow.Ticks}",
                Correct = true,
                Chosen = 0
            });
        }
    }

    /// <summary>
    /// Ensures default values are set for missing fields on the <see cref="Item"/>.
    /// Populates AnswerExplanation, ResponseType, and QuestionText if they are empty.
    /// </summary>
    /// <param name="item">The item to update.</param>
    /// <param name="text">The raw text used to extract defaults.</param>
    private void EnsureDefaults(Item item, string text)
    {
        if (string.IsNullOrWhiteSpace(item.AnswerExplanation))
            item.AnswerExplanation = _tokenizer.ExtractSection(text, "Solution");

        if (string.IsNullOrWhiteSpace(item.ResponseType))
            item.ResponseType = "Unknown";

        if (string.IsNullOrWhiteSpace(item.QuestionText))
            item.QuestionText = "No question text parsed.";
    }
}
