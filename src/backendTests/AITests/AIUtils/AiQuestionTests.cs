/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using AI;
using Xunit;

namespace AITests;

public class AiQuestionTests
{
    private readonly AIUtils aiUtils = new();

    [Fact]
    public async Task AiQuestion_UserMessageContainsQuestionDataTag()
    {
        var messages = await aiUtils.AiQuestion("sys", "usr", []);
        Assert.Contains("<questionData>", AITestHelpers.GetProp(messages[1], "content"));
    }

    [Fact]
    public async Task AiQuestion_UserMessageContainsExampleQuestionFields()
    {
        var examples = new List<Tuple<string, string, List<(string, bool)>, string>>
            {
                Tuple.Create(
                    "What is OOP?",
                    "multiple_choice",
                    new List<(string, bool)> { ("Inheritance", true), ("Pasta", false) },
                    "OOP stands for Object Oriented Programming"
                )
            };

        var messages = await aiUtils.AiQuestion("sys", "usr", examples);
        var content = AITestHelpers.GetProp(messages[1], "content");

        Assert.Contains("What is OOP?", content);
        Assert.Contains("multiple_choice", content);
        Assert.Contains("Inheritance", content);
        Assert.Contains("Pasta", content);
        Assert.Contains("OOP stands for Object Oriented Programming", content);
    }
}