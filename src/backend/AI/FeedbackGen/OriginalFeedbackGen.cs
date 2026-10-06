﻿/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using AI;

namespace AI.FeedbackGen;

/// <summary>
/// Original pipeline for generating AI feedback
/// </summary>
public class OriginalFeedbackGen(
    IAIAPI aiapi,
    AIUtils aiUtils) : IFeedbackGen
{
    /// <summary>
    ///  Generates feedback based on a completed test. This implementation uses the original prompts and prompts it to GPT-5-mini once.
    /// </summary>
    /// <param name="results">A list of tuples, each containing four string values representing test result details.</param>
    /// <returns>An asynchronous enumerable of strings. It returns the stream of tokens that make up the generated feedback.</returns>
    public async IAsyncEnumerable<string> GenerateFeedback(
        List<Tuple<string, string, string, string>> results)
    {
        string baseDir = AppContext.BaseDirectory;

        string filePath = Path.Combine(
            baseDir, "../../../../..", "backend", "AI", "Prompts", "feedback_system.txt");

        using StreamReader reader = new StreamReader(filePath);
        string feedbackSystemText = reader.ReadToEnd();

        string filePath2 = Path.Combine(
            baseDir, "../../../../..", "backend", "AI", "Prompts", "feedback_user.txt");

        using StreamReader reader2 = new StreamReader(filePath2);
        string feedbackUserText = reader2.ReadToEnd();

        var messages = await aiUtils.AiFeedback(results, feedbackSystemText, feedbackUserText);

        await foreach (var token in aiapi.PromptAIStream(messages))
        {
            yield return token;
        }
    }
}
