# AA — Adaptive Algorithms

AA is the main algorithm project that provides a standard location for algorithms to be implemented. Each type of algorithm implements an interface as per the Strategy design pattern, and can be uniquely identified using reflection via the `Algorithm` attribute. The algorithms are each supposed to have a "Simple" version that can be used as a placeholder, swappable by more complex implementations. Add a new implementation by creating a new class and giving it an `Algorithm` attribute. From there, it can be instantiated using the `AlgorithmRegistry<T>` class by using `registry.Get("Key")`.

---

## Structure

Here, the bold keywords are to be interpreted as per the RFC 2119 documentation standard. 

Every algorithm type in the AA project **must** follow this structure to be compatible with the registry and DI system.

### 1. Define an Interface

Each category of algorithm **must** have a corresponding interface in its own folder. The interface defines the contract all implementations **must** follow:

```
AA/
└── DefaultProficiencyAlgorithm/
    └── IDefaultProficiencyAlgorithm.cs
```

### 2. Implement the Interface

Each implementation **must** be tagged with `[Algorithm("Key Name")]`, where the key is a unique human-readable string used to retrieve it from the registry. The implementation **must** be placed in the same folder as its interface:

```csharp
[Algorithm("Simple DPA")]
public class SimpleDPA(AppDbContext context) : IDefaultProficiencyAlgorithm
{
    public async Task DoSomething(int userId) { ... }
}
```

Dependencies such as `AppDbContext` **should** be injected via the primary constructor — the DI container resolves these automatically.

### 3. Register in the API

In `Program.cs`, all implementations of an interface **must** be registered using the `AddAlgorithms<T>` extension method:

```csharp
var aaAssembly = typeof(AlgorithmServiceAdder).Assembly; // Already present in Program.cs, no need to add
builder.Services.AddAlgorithms<IDefaultProficiencyAlgorithm>(aaAssembly); // This must be added for each new Algorithm interface
```

The registry itself is registered once using open generics, covering all algorithm types:

```csharp
builder.Services.AddScoped(typeof(IAlgorithmRegistry<>), typeof(AlgorithmRegistry<>)); // Already present in Program.cs
```

### 4. Use via the registry

To use an algorithm, inject `IAlgorithmRegistry<T>` and call `registry.Get("Key Name")`:

```csharp
public class SomeService(IAlgorithmRegistry<ISomeAlgorithm> registry)
{
    public async Task DoWork(Input input)
    {
        var algorithm = registry.Get("Some Implementation");
        await algorithm.DoSomething(input);
    }
}
```

### Adding a New Implementation

1. Create a new class in the relevant algorithm folder
2. Implement the interface
3. Tag it with `[Algorithm("Your Key")]`

No changes to `Program.cs` or the registry are needed — the assembly scan picks it up automatically.

## Key resolvers for the adaptive algorihtms (NOT YET IMPLEMENTED)

By default, the algorithm key is resolved inline above the function call. A solid next step would be to implement an interface for keyresolvers, that base the choice of key on some configuration file or eventually A/B testing. The key resolver should be placed in the root of the AA project. 

To change how the key is resolved — for example, to introduce A/B testing — implement an interface similar to the following:
```csharp
public class AbTestingKeyResolver(IAbTestingService abTesting, int userId) 
    : IAlgorithmKeyResolver
{
    public string Resolve()
    {
        return abTesting.GetVariant(userId, "algorithm-variant");
    }
}
```

Then change the resolver in `Program.cs`:
```csharp
// Before
builder.Services.AddScoped<IAlgorithmKeyResolver, ConfigAlgorithmKeyResolver>();

// After
builder.Services.AddScoped<IAlgorithmKeyResolver, AbTestingKeyResolver>();
```

No other code changes are needed — the service, registry, and algorithm implementations are all unaffected. Algorithms are then called by using 
