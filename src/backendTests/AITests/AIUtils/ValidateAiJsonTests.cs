/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using AI;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Xunit;

namespace AITests;

public class ValidateAiJsonTests
{
    private readonly AIUtils aiUtils = new();

    private static JSchema SimpleSchema() => JSchema.Parse("""
            {
                "type": "object",
                "properties": {
                    "name": { "type": "string" }
                },
                "required": ["name"]
            }
            """);

    private static JSchema QuestionSchema() => JSchema.Parse("""
            {
                "$schema": "http://json-schema.org/draft-07/schema#",
                "type": "object",
                "required": ["Metainformation", "Question", "Options", "Answer_explanation"],
                "properties": {
                    "Metainformation": {
                        "type": "object",
                        "required": ["item_type", "response_type", "language", "level"],
                        "properties": {
                            "item_type": { "type": "string" },
                            "response_type": { "type": "string" },
                            "language": { "type": "string" },
                            "level": { "type": "string" }
                        },
                        "additionalProperties": true
                    },
                    "Question": { "type": "string" },
                    "Options": {
                        "type": "array",
                        "items": {
                            "type": "object",
                            "required": ["answer_text", "correct"],
                            "properties": {
                                "answer_text": { "type": "string" },
                                "correct": { "type": "boolean" }
                            }
                        }
                    },
                    "Answer_explanation": { "type": "string" }
                },
                "additionalProperties": false
            }
            """);

    [Fact]
    public void ValidateAiJson_ReturnsTrueForValidToken()
    {
        var token = JToken.Parse("""{ "name": "Utrecht" }""");
        Assert.True(aiUtils.ValidateAiJson(token, SimpleSchema()));
    }

    [Fact]
    public void ValidateAiJson_ThrowsWhenRequiredFieldMissing()
    {
        var token = JToken.Parse("""{ "other": 123 }""");
        var ex = Assert.Throws<Exception>(() => aiUtils.ValidateAiJson(token, SimpleSchema()));
        Assert.Contains("AI JSON did not match schema", ex.Message);
    }

    [Fact]
    public void ValidateAiJson_ThrowsWhenTypeIsWrong()
    {
        var token = JToken.Parse("""{ "name": 42 }""");
        var ex = Assert.Throws<Exception>(() => aiUtils.ValidateAiJson(token, SimpleSchema()));
        Assert.Contains("AI JSON did not match schema", ex.Message);
    }

    [Fact]
    public void ValidateAiJson_ErrorMessageContainsSchemaViolations()
    {
        var strictSchema = JSchema.Parse("""
                {
                    "type": "object",
                    "properties": {
                        "age": { "type": "integer", "minimum": 0 }
                    },
                    "required": ["age"]
                }
                """);

        var token = JToken.Parse("""{ "age": -5 }""");
        var ex = Assert.Throws<Exception>(() => aiUtils.ValidateAiJson(token, strictSchema));
        Assert.Contains("AI JSON did not match schema", ex.Message);
    }
    // --- Real-world question payloads ---

    [Fact]
    public void ValidateAiJson_ReturnsTrueForValidRealWorldQuestionToken()
    {
        var token = JToken.Parse("""
                {
                    "Metainformation": {
                        "item_type": "Multiple_choice",
                        "response_type": "Conceptual",
                        "language": "Dutch",
                        "level": "Statistical Literacy"
                    },
                    "Question": "<p>Wat is het gemiddelde(mean) cijfer van John?</p>",
                    "Options": [
                        { "answer_text": "<p>6.50</p>", "correct": false },
                        { "answer_text": "<p>6.67</p>", "correct": true },
                        { "answer_text": "<p>7.00</p>", "correct": false },
                        { "answer_text": "<p>6.70</p>", "correct": false }
                    ],
                    "Answer_explanation": "<p>Gemiddelde: 40 / 6 = 6.67.</p>"
                }
                """);
        Assert.True(aiUtils.ValidateAiJson(token, QuestionSchema()));
    }

    [Fact]
    public void ValidateAiJson_ThrowsWhenAnswerExplanationMissing()
    {
        var token = JToken.Parse("""
                {
                    "Metainformation": {
                        "item_type": "Multiple_choice",
                        "response_type": "Conceptual",
                        "language": "Dutch",
                        "level": "Statistical Literacy"
                    },
                    "Question": "<p>Wat is het gemiddelde cijfer van John?</p>",
                    "Options": [
                        { "answer_text": "<p>6.50</p>", "correct": false },
                        { "answer_text": "<p>6.67</p>", "correct": true }
                    ]
                }
                """);
        var ex = Assert.Throws<Exception>(() => aiUtils.ValidateAiJson(token, QuestionSchema()));
        Assert.Contains("AI JSON did not match schema", ex.Message);
    }

}