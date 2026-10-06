/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */
using System;
using System.Reflection.PortableExecutable;
using Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace AI;

/// <summary>
/// Summary description for Class1
/// </summary>
public class AIUtils
{
    /// <summary>
    /// Validates a JToken against a given JSON schema.
    /// </summary>
    /// <param name="token">The JToken to be validated</param>
    /// <param name="schema">The JSON schema the <see cref="JToken"/> needs to follow.</param>
    /// <returns>A boolean whether the <paramref name="token"/> follows the <paramref name="schema"/>.</returns>
    /// <exception cref="Exception">
    /// Thrown when the JSON is invalid or does not match the schema.
    /// </exception>
    public bool ValidateAiJson(JToken token, JSchema schema)
    {
        if (!token.IsValid(schema, out IList<string> errors))
        {
            string errorMessages = string.Join("; ", errors);
            throw new Exception($"AI JSON did not match schema: {errorMessages}");
        }
        
        return true;
    }

    /// <summary>
    /// Builds a message payload for AI-based question generation.
    /// </summary>
    /// <param name="feedbackSystemText">System-level instructions for the AI.</param>
    /// <param name="feedbackUserText">User-level prompt content.</param>
    /// <returns>
    /// A list of message objects formatted for submission to the AI API.
    /// </returns>
    public async Task<List<object>> AiQuestion(string feedbackSystemText, string feedbackUserText, List<Tuple<string, string, List<(string, bool)>, string>> exampleQuestions)
    {
        string questionData = "<questionData>\n";
        
        foreach (var item in exampleQuestions)
        {
            var answerOptions = "";
            foreach (var answer in item.Item3)
            {
                answerOptions += $"answer_option: {answer.Item1}, correct: {answer.Item2},\n";
            }

            questionData +=
                $"question: {item.Item1},\n" +
                $"question_type: {item.Item2},\n" +
                answerOptions +
                $"answer_explanation: {item.Item4}.\n\n";
        }

        var message = new List<object>
        {
            new { role = "system", content = feedbackSystemText },
            new { role = "user", content = feedbackUserText + questionData }
        };

        return message;
    }

    /// <summary>
    /// Parses a validated AI JSON token into an <see cref="Item"/> entity.
    /// </summary>
    /// <param name="token">
    /// A validated AI response token containing question and solution data.
    /// </param>
    /// <returns>
    /// A newly created <see cref="Item"/> populated from the AI response.
    /// </returns>
    public Item ParseQuestionFromJson(JToken token)
    {
        var item = new Item
        {
            Active = true,
            Source = Item.ItemSource.LLM,
            AppearanceCount = 0,
            Lang = token["Metainformation"]?["language"]?.ToString()?.ToLower() == "english"
                ? Item.Language.English
                : Item.Language.Dutch,
            Level = token["Metainformation"]?["level"]?.ToString(),
            ResponseType = token["Metainformation"]?["response_type"]?.ToString() ?? "Unknown",
            Type = token["Metainformation"]?["item_type"]?.ToString()?.ToLower() switch
            {
                "num" => Item.ItemType.Numerical,
                string s when s.Contains("open") => Item.ItemType.Open,
                _ => Item.ItemType.MultipleChoice
            },
            QuestionText = token["Question"]?.ToString() ?? "No question text",
            Answers = token["Options"]?.ToObject<List<ItemAnswer>>()?.Select((a, index) => 
                {
                    a.AnswerIdentifier = $"qti_{DateTime.UtcNow.Ticks}";
                    a.Chosen = 0;
                    return a;
                }).ToList() ?? new List<ItemAnswer>(),
            AnswerExplanation = token["Answer_explanation"]?.ToString() ?? "No answer explanation"
        };
        return item;
    }


    /// <summary>
    /// Builds an AI prompt for generating feedback based on test results.
    /// </summary>
    /// <param name="results">
    /// A list of tuples containing:
    /// question text, student answer, correct answer, and answer explanation.
    /// </param>
    /// <param name="feedbackSystemText">
    /// System-level instructions that guide the AI’s feedback behavior.
    /// </param>
    /// <param name="feedbackUserText">
    /// User-level instructions describing the feedback to generate.
    /// </param>
    /// <returns>
    /// A list of message objects formatted for submission to the AI API.
    /// </returns>
    public async Task<List<object>> AiFeedback(
        List<Tuple<string, string, string, string>> results,
        string feedbackSystemText,
        string feedbackUserText)
    {
        var message = new List<object>
        {
            new { role = "system", content = feedbackSystemText }
        };

        string questions = "<questionData>\n";

        foreach (var item in results)
        {
            questions +=
                $"question: {item.Item1},\n" +
                $"student_answer: {item.Item2},\n" +
                $"correct_answer: {item.Item3}.\n" +
                $"answer_explanation: {item.Item4}.\n\n";
        }

        message.Add(new
        {
            role = "user",
            content = feedbackUserText + "\n\n" + questions + "</questionData>"
        });

        return message;
    }

    /// <summary>
    /// Formats structured AI feedback JSON into a human-readable string.
    /// </summary>
    /// <param name="token">
    /// A validated AI feedback JSON token.
    /// </param>
    /// <returns>
    /// A formatted feedback string suitable for display or storage.
    /// </returns>
    public string FormatAiFeedback(JToken token)
    {
        var strengths = token["overallStrengths"]!.ToObject<List<string>>();
        var improvements = token["areasForImprovement"]!.ToObject<List<string>>();
        var guidance = token["generalGuidance"]!.ToObject<List<string>>();
        var openQuestions = token["specificQuestionsNeedingAttention"]!
            .ToObject<List<string>>();

        var feedbackParts = new List<string>
        {
            "STRENGTHS:" + Environment.NewLine + string.Join(" ", strengths),
            "AREAS FOR IMPROVEMENT:" + Environment.NewLine + string.Join(" ", improvements),
            "GENERAL TIPS:" + Environment.NewLine + string.Join(" ", guidance)
        };

        if (openQuestions != null && openQuestions.Count > 0)
        {
            feedbackParts.Add(
                "OPEN QUESTIONS THAT REQUIRE ATTENTION:" +
                Environment.NewLine +
                string.Join(" ", openQuestions)
            );
        }

        return string.Join(Environment.NewLine + Environment.NewLine, feedbackParts);
    }

    /// <summary>
    /// Reads a prompt template from a file, replaces placeholders for topicId and topicName,
    /// and returns the resulting message content for AI question generation.
    /// </summary>
    /// <param name="file">The filename of the prompt template.</param>
    /// <param name="topicId">The numeric ID of the topic to include in the prompt.</param>
    /// <param name="topicName">The name of the topic to include in the prompt.</param>
    /// <returns>
    /// The message with placeholders replaced, ready for submission to the AI.
    /// </returns>
    public string GetRoleMessageContent(string file, int topicId, string topicName, int questionCount = 0)
    {
        string baseDir = AppContext.BaseDirectory;
        string filePath = Path.Combine(
            baseDir, "../../../../..", "backend", "AI", "Prompts", file);
        using StreamReader reader = new StreamReader(filePath);
        return reader.ReadToEnd()
            .Replace("{topicId}", topicId.ToString())
            .Replace("{topicName}", topicName)
            .Replace("{questionCount}", questionCount.ToString());
    }
}
