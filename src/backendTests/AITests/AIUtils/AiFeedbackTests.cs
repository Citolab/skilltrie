/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using AI;
using Xunit;

namespace AITests;

public class AiFeedbackTests
{
    private readonly AIUtils aiUtils = new();

    [Fact]
    public async Task AiFeedback_UserMessageContainsQuestionDataTags()
    {
        var results = new List<Tuple<string, string, string, string>>
            {
                Tuple.Create("What is 2+2?", "5", "4", "Basic arithmetic")
            };
        var messages = await aiUtils.AiFeedback(results, "sys", "usr");
        var content = AITestHelpers.GetProp(messages[1], "content");

        Assert.Contains("<questionData>", content);
        Assert.Contains("</questionData>", content);
    }

    [Fact]
    public async Task AiFeedback_UserMessageContainsAllResultFields()
    {
        var results = new List<Tuple<string, string, string, string>>
            {
                Tuple.Create("Q text", "student ans", "correct ans", "explanation here")
            };
        var messages = await aiUtils.AiFeedback(results, "sys", "usr");
        var content = AITestHelpers.GetProp(messages[1], "content");

        Assert.Contains("Q text", content);
        Assert.Contains("student ans", content);
        Assert.Contains("correct ans", content);
        Assert.Contains("explanation here", content);
    }
}