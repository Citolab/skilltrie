/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

// using Models;
using System.Text.Json;
using System.Text;
using Microsoft.Extensions.Logging;

namespace AI;


public interface IAIAPI
{
    Task<string> PromptAI(List<object> messages, int maxCompletionTokens = 8000);
    IAsyncEnumerable<string> PromptAIStream(List<object> messages, int maxCompletionTokens = 8000);
}

public class AIAPI : IAIAPI
{
    private readonly string _apiKey;
    private readonly string _url;
    private readonly ILogger<AIAPI> _logger;

    /// <summary>
    /// Constructor for AIAPI.
    /// Initializes logging, reads configuration from environment variables,
    /// and creates a tokenizer and parser for AI items.
    /// </summary>
    public AIAPI(ILogger<AIAPI> logger)
    {
        string endpoint = Environment.GetEnvironmentVariable("BACKEND_AI_ENDPOINT")
            ?? throw new Exception("BACKEND_AI_ENDPOINT not defined in Environment variables");
        string deployment = Environment.GetEnvironmentVariable("BACKEND_AI_DEPLOYMENT_NAME")
            ?? throw new Exception("BACKEND_AI_DEPLOYMENT_NAME not defined in Environment variables");
        string apiVersion = Environment.GetEnvironmentVariable("BACKEND_AI_VERSION")
            ?? throw new Exception("BACKEND_AI_VERSION not defined in Environment variables");
        _apiKey = Environment.GetEnvironmentVariable("BACKEND_AI_KEY")
            ?? throw new Exception("BACKEND_AI_KEY not defined in Environment variables");

        _url = $"{endpoint}/openai/v1/chat/completions";
        _logger = logger;
    }

    /// <summary>
    /// Sends a list of messages to the AI endpoint.
    /// Builds the JSON request body, sends a POST request,
    /// validates the response, and extracts the content string.
    /// </summary>
    /// <param name="messages">The list of messages (role + content) to send to the AI.</param>
    /// <param name="maxCompletionTokens">Maximum number of tokens for the AI response (default 8000).</param>
    /// <returns>The content string returned by the AI.</returns>
    public async Task<string> PromptAI(List<object> messages, int maxCompletionTokens = 8000)
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("api-key", _apiKey);

        var bodyObject = new { model= "gpt-5.4-nano",messages, max_completion_tokens = maxCompletionTokens };
        string body = JsonSerializer.Serialize(bodyObject);

        var content = new StringContent(body, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await httpClient.PostAsync(_url, content);

        string result = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Error occurred calling the AI API, response: " + result);
        }

        var root = JsonSerializer.Deserialize<Root>(result);
        if (root == null)
        {
            throw new Exception("Deserialization failed.");
        }

        _logger.LogInformation($"prompted AIAPI, returned {response.StatusCode}, result: {result}");
        string contentResult = root.choices[0].message.content;
        return contentResult;
    }


    /// <summary>
    /// Sends a list of messages to the AI endpoint.
    /// Builds the JSON request body, sends a POST request,
    /// streams back the response.
    /// </summary>
    /// <param name="messages">The list of messages (role + content) to send to the AI.</param>
    /// <param name="maxCompletionTokens">Maximum number of tokens for the AI response (default 8000).</param>
    /// <returns>Context tokens as they arrive.</returns>
    public async IAsyncEnumerable<string> PromptAIStream(List<object> messages, int maxCompletionTokens = 3000)
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("api-key", _apiKey);

        var bodyObject = new { model= "gpt-5.4-nano", messages, max_completion_tokens = maxCompletionTokens, stream = true };
        string body = JsonSerializer.Serialize(bodyObject);

        var content = new StringContent(body, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, _url) { Content = content };

        using var response = await httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError($"Error occurred calling the AI Stream API");
            throw new Exception("Error occurred calling the AI Stream API");
        }

        var result = await response.Content.ReadAsStreamAsync();
        var sr = new StreamReader(result);
        var line = string.Empty;

        while ((line = await sr.ReadLineAsync()) is not null)
        {
            if(!line.StartsWith("data: ") || string.IsNullOrWhiteSpace(line))
                continue;

            string data = line["data: ".Length..];

            if(data == "[DONE]")
                yield break;

            var processedData = JsonSerializer.Deserialize<StreamRoot>(data);

            if (processedData?.choices == null || processedData.choices.Count == 0)
                continue;

            string? res = processedData.choices[0].delta.content;
            if(res == null)
                continue;

            yield return res;
        }

        _logger.LogInformation($"Completed streaming AI, returned {response.StatusCode}");
    }

    /// <summary>
    /// Reads topicprompt.txt, replaces placeholders for topicId and topicName,
    /// parses #START_MESSAGE and #END_MESSAGE blocks,
    /// and builds a list of messages (role + content) for the AI.
    /// </summary>
    /// <param name="topicId">The numeric ID of the topic.</param>
    /// <param name="topicName">The name of the topic.</param>
    /// <returns>A list of objects containing role and content for the topic prompt.</returns>
    private List<object> GenerateTopicPrompt(int topicId, string topicName)
    {
        var messages = new List<object>();
        try
        {
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Prompts/topicprompt.txt");

            using StreamReader reader = new StreamReader(filePath);
            string promptText = reader.ReadToEnd();

            promptText = promptText.Replace("{topicId}", topicId.ToString())
                .Replace("{topicName}", topicName);

            int messageStartIndex = promptText.IndexOf("#START_MESSAGE ", StringComparison.Ordinal);

            while (messageStartIndex != -1)
            {

                promptText = promptText.Substring(messageStartIndex + "#START_MESSAGE ".Length);

                string role;
                if (promptText.StartsWith("user\r\n", StringComparison.Ordinal))
                {
                    role = "user";
                    promptText = promptText.Substring("user\r\n".Length);
                }
                else if (promptText.StartsWith("system\r\n", StringComparison.Ordinal))
                {
                    role = "system";
                    promptText = promptText.Substring("system\r\n".Length);
                }
                else
                {
                    throw new Exception("topicprompt.txt incorrectly formatted");
                }

                int messageEndIndex = promptText.IndexOf("#END_MESSAGE", StringComparison.Ordinal);
                if (messageEndIndex == -1)
                {
                    throw new Exception("topicprompt.txt incorrectly formatted");
                }

                string messageText = promptText.Substring(0, messageEndIndex);

                int nextStart = messageEndIndex + "#END_MESSAGE\r\n".Length;
                promptText = nextStart >= promptText.Length ? string.Empty : promptText.Substring(nextStart);

                messages.Add(new { role, content = messageText });

                messageStartIndex = promptText.IndexOf("#START_MESSAGE ", StringComparison.Ordinal);
            }
        }
        catch (IOException e)
        {
            throw new Exception("Error reading topicprompt.txt", e);
        }
        catch (Exception ex)
        {
            throw new Exception("Error reading topicprompt.txt", ex);
        }

        return messages;
    }
}
