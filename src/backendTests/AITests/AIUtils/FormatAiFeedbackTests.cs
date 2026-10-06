/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using AI;
using Xunit;

namespace AITests;

public class FormatAiFeedbackTests
{
    private readonly AIUtils aiUtils = new();

    [Fact]
    public void FormatAiFeedback_IncludesStrengthsContent()
    {
        var token = AITestHelpers.BuildFeedbackToken(strengths: ["Good understanding of loops"]);
        Assert.Contains("Good understanding of loops", aiUtils.FormatAiFeedback(token));
    }

    [Fact]
    public void FormatAiFeedback_IncludesImprovementsContent()
    {
        var token = AITestHelpers.BuildFeedbackToken(improvements: ["Review recursion"]);
        Assert.Contains("Review recursion", aiUtils.FormatAiFeedback(token));
    }

    [Fact]
    public void FormatAiFeedback_IncludesGuidanceContent()
    {
        var token = AITestHelpers.BuildFeedbackToken(guidance: ["Practice daily"]);
        Assert.Contains("Practice daily", aiUtils.FormatAiFeedback(token));
    }

    [Fact]
    public void FormatAiFeedback_IncludesOpenQuestionsContentWhenPresent()
    {
        var token = AITestHelpers.BuildFeedbackToken(openQuestions: ["Revisit question 3"]);
        Assert.Contains("Revisit question 3", aiUtils.FormatAiFeedback(token));
    }

    [Fact]
    public void FormatAiFeedback_ExcludesOpenQuestionsContentWhenEmpty()
    {
        // Unique sentinel value — if it appears, the empty list was rendered anyway
        var token = AITestHelpers.BuildFeedbackToken(openQuestions: []);
        Assert.DoesNotContain("SENTINEL_OPEN_Q", aiUtils.FormatAiFeedback(token));
    }

    [Fact]
    public void FormatAiFeedback_AllContentPresentInSingleOutput()
    {
        var token = AITestHelpers.BuildFeedbackToken(
            strengths: ["Strong algebra skills"],
            improvements: ["Work on statistics"],
            guidance: ["Review chapter 3"],
            openQuestions: ["Question 5 needs attention"]
        );

        var result = aiUtils.FormatAiFeedback(token);

        Assert.Contains("Strong algebra skills", result);
        Assert.Contains("Work on statistics", result);
        Assert.Contains("Review chapter 3", result);
        Assert.Contains("Question 5 needs attention", result);
    }

    [Fact]
    public void FormatAiFeedback_MultipleItemsInSectionAllAppear()
    {
        var token = AITestHelpers.BuildFeedbackToken(strengths: ["Strength one", "Strength two", "Strength three"]);
        var result = aiUtils.FormatAiFeedback(token);

        Assert.Contains("Strength one", result);
        Assert.Contains("Strength two", result);
        Assert.Contains("Strength three", result);
    }

}
