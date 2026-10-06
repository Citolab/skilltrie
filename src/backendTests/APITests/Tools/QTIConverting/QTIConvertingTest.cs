/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using API.Tools.QTIConverting;
using Models;

namespace APITests.QTIConvertingTests;

public class QTIConvertingTest
{
    [Fact]
    public void AssessmentXMLGenerationTest()
    {
        // mock items
        Item[] items = [
            new() { Id = 1 },
            new() { Id = 2 },
            new() { Id = 3 }
        ];

        string assessmentString = QTIXML.GenerateAssesment(items);
        string expectedAssessmentString =
            File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Tools/QTIConverting/expectedstrings", "assessment.txt"));

        Assert.Equal(expectedAssessmentString, assessmentString);
    }

    [Fact]
    public void MultiChoiceItemXmlGenerationTest()
    {
        ItemAnswer[] answers = [
            new()
            {
                Id = 1,
                AnswerText = "<p>both statements are true</p>",
                AnswerIdentifier = "qti_21_items_all_238788197_section_0001_item_1_schoice_1_6240525192",
                Correct = true
            },
            new()
            {
                Id = 2,
                AnswerText = "<p>only statement 1 is true</p>",
                AnswerIdentifier = "qti_21_items_all_238788197_section_0001_item_1_schoice_2_3435843680",
                Correct = false
            },
            new()
            {
                Id = 3,
                AnswerText = "<p>only statement 2 is true</p>",
                AnswerIdentifier = "qti_21_items_all_238788197_section_0001_item_1_schoice_3_8045303654",
                Correct = false
            },
            new()
            {
                Id = 4,
                AnswerText = "<p>both statements are false</p>",
                AnswerIdentifier = "qti_21_items_all_238788197_section_0001_item_1_schoice_4_1822178943",
                Correct = false
            }
        ];


        Item multiChoiceItem = new()
        {
            Id = 1,
            Type = Item.ItemType.MultipleChoice,
            Source = Item.ItemSource.Databank,
            ResponseType = "conceptual",
            AnswerExplanation =
                "<p>Claim 1 is true because sphericity looks at whether the variances of the difference scores differ. In the case of 2 levels, there is only one difference score, so this assumption does not apply.</p><p>Claim 2 is also true because you can perform repeated measures with a MANOVA, but the assumption of sphericity is not required in this analysis.</p><br /><ul><li>both statements are true<br />True</li><li>only statement 1 is true<br />False</li><li>only statement 2 is true<br />False</li><li>both statements are false<br />False</li></ul>",
            QuestionText =
                @"<p>In a one-way repeated measures ANOVA, a researcher needs to test, amongst other assumptions, the sphericity assumption. Consider the following statements about the sphericity assumption and answer the subsequent question.</p><ol><li><p>This assumption does not apply when the within-subjects factor only has two levels.</p></li><li><p>When this assumption is severely violated, a researcher can use a repeated-measures multivariate test (MANOVA) as an alternative to the uncorrected F-test.</p></li></ol><p>Which statement is true?</p>",
            Answers = answers
        };

        string itemString = QTIXML.Convert(multiChoiceItem);

        string expectedItemString =
            File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Tools/QTIConverting/expectedstrings", "multiplechoice.txt"));

        Assert.Equal(itemString, expectedItemString);
    }

    [Fact]
    public void OpenItemXmlGenerationTest()
    {
        ItemAnswer answer = new()
        {
            Id = 370,
            AnswerText = "&quot;smaller than&quot;",
            AnswerIdentifier = "IdentifierOfAnInputField",
            Correct = true
        };

        Item openItem = new()
        {
            Id = 93,
            Type = Item.ItemType.Open,
            Source = Item.ItemSource.Databank,
            ResponseType = "conceptual",
            AnswerExplanation =
                "<p>When a variable is distributed negatively skewed, the mean is smaller than the median. When a variable is negatively skewed, the tail of the distribution is at the left side. The mean, and not the median is influenced by these small numbers and is therefore smaller than the median.</p>",
            QuestionText =
                "<p>Fill in: “larger than”, “smaller than” or “equal to”:When a variable is normally distributed the mean is …. the median.</p><p><qti-text-entry-interaction /></p>",
            Answers = [answer]
        };

        string itemString = QTIXML.Convert(openItem);

        string expectedItemString =
            File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Tools/QTIConverting/expectedstrings", "open.txt"));

        Assert.Equal(expectedItemString, itemString);
    }

    [Fact]
    public void NullItemTest()
    {
        Assert.Throws<NullReferenceException>(() => QTIXML.Convert(null));
    }

    [Fact]
    public void NoCorrectAnswersItemTest()
    {
        Item item = new()
        {
            Id = 1,
            Answers = [
                new() {
                    Correct = false
                },
                new () {
                    Correct = false
                }
            ]
        };

        Assert.Throws<ItemNoCorrectAnswers>(() => QTIXML.Convert(item));
    }

    [Fact]
    public void NoCorrectAnswersItemTest2()
    {
        Item item = new()
        {
            Id = 1,
            Answers = []
        };

        Assert.Throws<ItemNoCorrectAnswers>(() => QTIXML.Convert(item));
    }

    [Fact]
    public void NoAnswerIdentifierItemTest()
    {
        Item item = new()
        {
            Id = 1,
            Answers = [
                new() {
                    Correct = true,
                    AnswerIdentifier = ""
                }
            ]
        };

        Assert.Throws<ItemNoAnswerIdentifier>(() => QTIXML.Convert(item));
    }

    [Fact]
    public void NoAnswerIdentifierItemTest2()
    {
        Item item = new()
        {
            Id = 1,
            Answers = [
                new() {
                    Correct = true,
                    AnswerIdentifier = null
                }
            ]
        };

        Assert.Throws<ItemNoAnswerIdentifier>(() => QTIXML.Convert(item));
    }
}
