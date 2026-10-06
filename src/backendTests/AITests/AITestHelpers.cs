/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using Newtonsoft.Json.Linq;

namespace AITests;

public static class AITestHelpers
{

    public static string GetProp(object obj, string name) =>
        obj.GetType().GetProperty(name)?.GetValue(obj)?.ToString() ?? "";

    public static JToken BuildToken(
        string? language = "english",
        string? itemType = "multiple_choice",
        string? questionText = "Sample question?",
        string? answerExplanation = "Because reasons.",
        string? responseType = "MC",
        string? level = "beginner",
        bool includeOptions = true)
    {
        var meta = new JObject();
        if (language != null) meta["language"] = language;
        if (itemType != null) meta["item_type"] = itemType;
        if (responseType != null) meta["response_type"] = responseType;
        if (level != null) meta["level"] = level;

        var obj = new JObject { ["Metainformation"] = meta };

        if (questionText != null) obj["Question"] = questionText;
        if (answerExplanation != null) obj["Answer_explanation"] = answerExplanation;

        if (includeOptions)
        {
            obj["Options"] = new JArray
            {
                new JObject { ["answer_text"] = "Option A", ["correct"] = true },
                new JObject { ["answer_text"] = "Option B", ["correct"] = false }
            };
        }

        return obj;
    }

    public static JToken BuildFeedbackToken(
        List<string>? strengths = null,
        List<string>? improvements = null,
        List<string>? guidance = null,
        List<string>? openQuestions = null)
    {
        return JToken.FromObject(new
        {
            overallStrengths = strengths ?? ["Good effort"],
            areasForImprovement = improvements ?? ["Study more"],
            generalGuidance = guidance ?? ["Keep going"],
            specificQuestionsNeedingAttention = openQuestions ?? []
        });
    }

}