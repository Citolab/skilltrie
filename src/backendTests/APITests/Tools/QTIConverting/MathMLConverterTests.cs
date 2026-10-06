/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using API.Tools.MathMLConverter;
using Models;

namespace APITests.MathMLConverterTests;


public class MathMLConverterTest
{
    private MathMLConverter converter;

    public MathMLConverterTest()
    {
        converter = new MathMLConverter();
    }

    [Fact]
    public async Task LaTeXToMathMLConvertFractionTest()
    {
        Item item = new Item { QuestionText = "\\begin{math}\\end{math} bhdsj \\begin{math}\\end{math} dfsdf", 
                                AnswerExplanation="\\begin{math}\\end{math} bhdsj \\begin{math}\\end{math} dfsdf", 
                                Answers= new List<ItemAnswer>
                                {
                                    new ItemAnswer
                                    {
                                        Id = 1,
                                        ItemId = 1,
                                        AnswerText = "\\begin{math}\\end{math} bhdsj \\begin{math}\\end{math} dfsdf",
                                        Chosen = 0,
                                        Correct = true,
                                        AnswerIdentifier = "A"
                                    }
                                }
                            };
        await converter.ConvertItemsToMathML([item]);

        Assert.DoesNotContain("{math}", item.QuestionText);
        Assert.DoesNotContain("{math}", item.AnswerExplanation);
        foreach( ItemAnswer ans in item.Answers)
        {
            Assert.DoesNotContain("{math}", ans.AnswerText);
        }
    }
}
