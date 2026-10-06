/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

using AI;

namespace IntegrationTests;

public class MockAIAPI : IAIAPI
{
    private const string MockResponse = """
        [{
        "Metainformation": {
            "item_type": "Multiple_choice",
            "response_type": "Conceptual",
            "language": "Dutch",
            "level": "Statistical Literacy"
        },
        "Question": "<p>John heeft de resultaten van zijn laatste toets terug gehad: [5.4, 6.7, 8.2, 5.9, 7.3, 6.5] Wat is het gemiddelde(mean) cijfer van John?</p>",
        "Options": [
            {"answer_text": "<p>6.50</p>", "correct": false},
            {"answer_text": "<p>6.67</p>", "correct": true},
            {"answer_text": "<p>7.00</p>", "correct": false},
            {"answer_text": "<p>6.70</p>", "correct": false}
        ],
        "Answer_explanation": "<p>Gemiddelde: 40 / 6 = 6.67...</p>"
        }]
        """;
    public async Task<string> PromptAI(List<object> messages, int maxCompletionTokens = 8000)
    {
        return await Task.FromResult(MockResponse);
    }

    public async IAsyncEnumerable<string> PromptAIStream(List<object> messages, int maxCompletionTokens = 8000)
    {
        yield return MockResponse;
    }

}