
# Auto Docs

The [auto-docs](/auto-docs/index) folder contains automatically generated documentation for the backend. Although
"automatic" may -- at least partially -- be a bit of a misnomer, as any explanations of classes, methods, etc. are pulled
from the XML comments you as a developer write yourself.
\
\
The documentation is generated automatically using [DefaultDocumentation](https://www.nuget.org/packages/DefaultDocumentation).
Generally, each namespace, class, method, etc. will be given its own documentation page. A single page corresponds to a single `.md` file. DefaultDocumentation will automatically include any [XML comments](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/documentation-comments) you add to aforementioned namespaces, classes, methods.
 
An example of said XML comments:
```csharp{1-6}
/// <summary>
/// Extract multiple items, acts as a wrapper for GenerateAssesmentXML
/// </summary>
/// <param name="context">Database Context</param>
/// <param name="items">Items to be parsed</param>
/// <returns>qti-xml assessment file</returns>
public static string ExtractItems(AppDbContext context, List<Item> items) {}
```

## Generating automatic documentation

Generating automatic documentation is generally **not** something you have to do manually; the CI/CD pipeline on GitLab is
supposed to do this automatically on every commit to `main`. In fact, the autodocs are added to the `.gitignore` to avoid human
intervention as much as possible.
\
Nevertheless, if you want to generate it for yourself there are **two** ways:

::: danger
Generating automatic documentation on Windows is currently *not possible*. This is due to specific characters (e.g. `<`, `>`, `\`, etc.)
being present in filenames that are reserved on Windows.
:::

1. 
    Run `npm run docs:autogen` in the `docs` folder.
2. The manual approach:
    - install the defaultdocumentation CLI tool with `dotnet tool install DefaultDocumentation.Console -g`. The `-g` flag installs it globally as a CLI tool on your machine.
    - run `dotnet build` in the `backend` folder. `dotnet run` is fine too, but you need not have the backend running for this.
    - run `defaultdocumentation --ConfigurationFilePath [path-to-config]`, the config file should be in `docs/autodocsconfig/`.

For both of these approaches, DefaultDocumentation's config is configured to output the generated documentation to `docs/documentation/auto-docs/`

### Adjusting DefaultDocumentation's config

The configuration file for DefaultDocumentation specifies which parts of the project should have documentation generated
automatically, and in what way that should happen. The configuration file is located in `docs/autodocsconfig/`. The configurable
options are available [here](https://www.nuget.org/packages/DefaultDocumentation).
