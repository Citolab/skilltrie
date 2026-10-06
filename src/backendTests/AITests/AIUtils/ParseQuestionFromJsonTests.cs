/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using AI;
using Models;
using Xunit;
using Newtonsoft.Json.Linq;

namespace AITests;

public class ParseQuestionFromJson_Language
{
    private readonly AIUtils aiUtils = new();

    [Fact]
    public void ParseQuestionFromJson_SetsEnglishWhenLanguageIsEnglish()
    {
        var token = JToken.Parse("""
                {
                    "Metainformation": { "language": "english" }
                }
                """);
        Assert.Equal(Item.Language.English, aiUtils.ParseQuestionFromJson(token).Lang);
    }

    [Fact]
    public void ParseQuestionFromJson_SetsEnglishCaseInsensitive()
    {
        var token = JToken.Parse("""
                {
                    "Metainformation": { "language": "ENGLISH" }
                }
                """);
        Assert.Equal(Item.Language.English, aiUtils.ParseQuestionFromJson(token).Lang);
    }

    [Fact]
    public void ParseQuestionFromJson_SetsDutchWhenLanguageIsDutch()
    {
        var token = JToken.Parse("""
                {
                    "Metainformation": { "language": "dutch" }
                }
                """);
        Assert.Equal(Item.Language.Dutch, aiUtils.ParseQuestionFromJson(token).Lang);
    }

    [Fact]
    public void ParseQuestionFromJson_DefaultsToDutchForUnknownLanguage()
    {
        var token = JToken.Parse("""
                {
                    "Metainformation": { "language": "french" }
                }
                """);
        Assert.Equal(Item.Language.Dutch, aiUtils.ParseQuestionFromJson(token).Lang);
    }

    [Fact]
    public void ParseQuestionFromJson_DefaultsToDutchWhenLanguageMissing()
    {
        var token = JToken.Parse("""
                {
                    "Metainformation": {}
                }
                """);
        Assert.Equal(Item.Language.Dutch, aiUtils.ParseQuestionFromJson(token).Lang);
    }
}

// =========================================================
// ParseQuestionFromJson — item type mapping
// =========================================================

public class ParseQuestionFromJson_ItemType
{
    private readonly AIUtils aiUtils = new();

    [Fact]
    public void ParseQuestionFromJson_SetsNumericalForNum()
    {
        var token = JToken.Parse("""
                {
                    "Metainformation": { "item_type": "num" }
                }
                """);
        Assert.Equal(Item.ItemType.Numerical, aiUtils.ParseQuestionFromJson(token).Type);
    }

    [Fact]
    public void ParseQuestionFromJson_SetsOpenForOpen()
    {
        var token = JToken.Parse("""
                {
                    "Metainformation": { "item_type": "open" }
                }
                """);
        Assert.Equal(Item.ItemType.Open, aiUtils.ParseQuestionFromJson(token).Type);
    }

    [Fact]
    public void ParseQuestionFromJson_SetsOpenForOpenEnded()
    {
        var token = JToken.Parse("""
                {
                    "Metainformation": { "item_type": "open_ended" }
                }
                """);
        Assert.Equal(Item.ItemType.Open, aiUtils.ParseQuestionFromJson(token).Type);
    }

    [Fact]
    public void ParseQuestionFromJson_SetsOpenForOpenWithPrefix()
    {
        var token = JToken.Parse("""
                {
                    "Metainformation": { "item_type": "open-question" }
                }
                """);
        Assert.Equal(Item.ItemType.Open, aiUtils.ParseQuestionFromJson(token).Type);
    }

    [Fact]
    public void ParseQuestionFromJson_DefaultsToMultipleChoiceForUnknownType()
    {
        var token = JToken.Parse("""
                {
                    "Metainformation": { "item_type": "weird_type" }
                }
                """);
        Assert.Equal(Item.ItemType.MultipleChoice, aiUtils.ParseQuestionFromJson(token).Type);
    }

    [Fact]
    public void ParseQuestionFromJson_DefaultsToMultipleChoiceWhenTypeMissing()
    {
        var token = JToken.Parse("""
                {
                    "Metainformation": {}
                }
                """);
        Assert.Equal(Item.ItemType.MultipleChoice, aiUtils.ParseQuestionFromJson(token).Type);
    }
}

