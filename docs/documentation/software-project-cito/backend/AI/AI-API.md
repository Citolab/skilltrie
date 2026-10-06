# AI API Documentation

The `AIAPI` class provides an interface for communicating with a Large Language Model (LLM).  
It generates prompts, sends them to an AI endpoint, and parses the output into strongly typed `Item` objects.

---

## Overview

The class performs three main tasks:

- **Prompt Construction** – Reads and processes `sampleprompt.txt`, converting it into an OpenAI-style message array.
- **LLM Communication** – Sends a `chat/completions` request to a configured AI deployment.
- **Output Parsing** – Converts AI output text into a structured `Item`.

---

## Environment Variables

| Variable | Purpose |
|---------|---------|
| `BACKEND_AI_ENDPOINT` | Base URL of the AI service |
| `BACKEND_AI_DEPLOYMENT_NAME` | Deployment/model name |
| `BACKEND_AI_VERSION` | API version |
| `BACKEND_AI_KEY` | API authentication key |

The final request URL is:

`{endpoint}/openai/deployments/{deployment}/chat/completions?api-version={version}`


---

## Request Flow

### 1. Asking the AI

- Constructs the prompt.
- Sends the prompt via the AI endpoint.
- Parses the AI's response into an Item.

### 2. Sending a Prompt

- Create a message list with roles (`system` and `user`).
- Serialize messages to JSON.
- POST to the AI endpoint with authentication.
- Receive response and extract the content string.

### 3. Prompt File Parsing

- `sampleprompt.txt` contains message blocks marked with:

````
#START_MESSAGE system
...
#END_MESSAGE

#START_MESSAGE user
...
#END_MESSAGE
````


- The system reads the file, detects roles, and produces a message array for the AI.

---

## AI Response Parsing

The AI always returns a structured response with three main components:

1. **Metainformation** — metadata describing the Item (Language, Level, Type, etc.)
2. **Question** — the full question text
3. **Solution** — the answer or solution text

The parser extracts these and maps them into an Item object.

---

## Error Handling

| Area | Possible Issue | Behavior |
|------|----------------|----------|
| Prompt parsing | Missing `#START_MESSAGE` or `#END_MESSAGE` | Throws an exception |
| API call | Server error / invalid config | Throws exception with raw response |
| JSON response | Missing or invalid structure | Parsing fails, exception thrown |
| Regex parsing | Missing metadata or sections | Returns empty values for missing parts |

---

## Example Prompt Structure

The request sent to the AI looks like:

```json
{
  "messages": [
    { "role": "system", "content": "..." },
    { "role": "user", "content": "..." }
  ],
  "max_completion_tokens": 8000
}
