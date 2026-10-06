/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

namespace AI;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

class Choice
{
    public ContentFilterResults content_filter_results { get; set; }
    public string finish_reason { get; set; }
    public int index { get; set; }
    public object logprobs { get; set; }
    public Message message { get; set; }
}

class CompletionTokensDetails
{
    public int accepted_prediction_tokens { get; set; }
    public int audio_tokens { get; set; }
    public int reasoning_tokens { get; set; }
    public int rejected_prediction_tokens { get; set; }
}

class ContentFilterResults
{
    public Hate hate { get; set; }
    public ProtectedMaterialCode protected_material_code { get; set; }
    public ProtectedMaterialText protected_material_text { get; set; }
    public SelfHarm self_harm { get; set; }
    public Sexual sexual { get; set; }
    public Violence violence { get; set; }
    public Jailbreak jailbreak { get; set; }
}

class Hate
{
    public bool filtered { get; set; }
    public string severity { get; set; }
}

class Jailbreak
{
    public bool filtered { get; set; }
    public bool detected { get; set; }
}

class Message
{
    public List<object> annotations { get; set; }
    public string content { get; set; }
    public object refusal { get; set; }
    public string role { get; set; }
}

class PromptFilterResult
{
    public int prompt_index { get; set; }
    public ContentFilterResults content_filter_results { get; set; }
}

class PromptTokensDetails
{
    public int audio_tokens { get; set; }
    public int cached_tokens { get; set; }
}

class ProtectedMaterialCode
{
    public bool filtered { get; set; }
    public bool detected { get; set; }
}

class ProtectedMaterialText
{
    public bool filtered { get; set; }
    public bool detected { get; set; }
}

class Root
{
    public List<Choice> choices { get; set; }
    public int created { get; set; }
    public string id { get; set; }
    public string model { get; set; }
    public string @object { get; set; }
    public List<PromptFilterResult> prompt_filter_results { get; set; }
    public object system_fingerprint { get; set; }
    public Usage usage { get; set; }
}

public class StreamRoot
{
    public List<StreamChoice> choices { get; set; }
}

public class StreamChoice
{
    public Delta delta { get; set; }
    public string? finish_reason { get; set; }
}

public class Delta
{
    public string? content { get; set; }
}

class SelfHarm
{
    public bool filtered { get; set; }
    public string severity { get; set; }
}

class Sexual
{
    public bool filtered { get; set; }
    public string severity { get; set; }
}

class Usage
{
    public int completion_tokens { get; set; }
    public CompletionTokensDetails completion_tokens_details { get; set; }
    public int prompt_tokens { get; set; }
    public PromptTokensDetails prompt_tokens_details { get; set; }
    public int total_tokens { get; set; }
}

class Violence
{
    public bool filtered { get; set; }
    public string severity { get; set; }
}
