# AI — Artificial Intelligence

Artificial Intelligence (AI) is used to generate items and feedback. This project exists to provide a location for AI-related classes that can be coupled to AIService using interfaces, this makes it possible to swap out the AI implementation without affecting the rest of the codebase.

---

## Structure

FeedbackGen and QuestionGen are the two main categories of AI algorithms, and each has its own folder with an interface and one or more implementations. 
The prompts used are stored in the `Prompts` folder. The `AIAPI` class handles communication with the AI endpoint. In the future, we want to change `IAIPI` to be able to use different AI-Models. 
Lastly the 'AIUtils' class contains some helper functions used for formatting.

### 1. Define an Interface

Each generation pipeline **must** have a corresponding interface in its own folder. The interface defines the contract all implementations **must** follow:

```
AI/
└── FeedbackGen/
    └── IFeedbackGen.cs
```

### 2. Implement the Interface

Each implementation **must** inherit the respective interface. The implementation **must** be placed in the same folder as its interface:

```csharp
public class FooFeedbackGen(
    IAIAPI aiapi,
    AIUtils aiUtils) : IFeedbackGen
{
    public async Task GenerateSomething() { ... }
}
```

Dependencies such as `IAIAPI` and `AIUtils` **should** be injected via the primary constructor — the DI container resolves these automatically.

### 3. Register in the API

In `Program.cs`, the implementation of an interface that you want to use in this build **must** be registered using the `builder.Services.AddScopeds<T>` method:

```csharp
builder.Services.AddScoped<IFeedbackGen, OriginalFeedbackGen>();
```

You can swap out the different implementations by changing which one is registered here. For example, to use `FooFeedbackGen` instead of `OriginalFeedbackGen`. In the future we want to be able to change this at runtime.

### Adding a New Implementation

1. Create a new class in the relevant generation folder
2. Implement the interface
3. Change the registration in `Program.cs` to the new implementation