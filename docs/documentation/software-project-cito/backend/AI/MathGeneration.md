# Mathematical text generation and conversion Documentation

Questions should have the ability to contain mathematical equations. These should be generated and stored in the LaTeX format. This has two reasons:
1. AI is more comfortable creating questions in LaTex. These formulas are of higher quality and more accurate compared to other options, such as MathML.
2. LaTeX is not verbose. This reduces overall token usage.

In the frontend the mathematical formulas need to be rendered in the MathML format, since this is native to HTML and the QTI-player is built around this assumption.

## Conversion 

In `itemController.cs`, the items for a test are gathered and converted in an assessment XML file before being sent to the frontend. Converting these formulas is not convient in the frontend, since you have to alter the already existing assessment format.

To convert LaTeX into MathML, we need a library capable of doing this. Most libraries are written in JavaScript and none on C#. As a result, we need to mock a JavaScript environment in the backend through the use of `Jint`. In this environment we can run the `Temml` library that allows this conversion.

### setup

A `Jint.Engine` and its paramters are set. 
```csharp
_engine = new Engine();
_engine.Execute("var document = { createElement: function() { return { setAttribute: function(){} }; } };");
_engine.Execute("var window = {};");
_engine.Execute("var exports = {};");
_engine.Execute("var module = { exports: exports };");

var scriptPath = Path.Combine(AppContext.BaseDirectory, "Tools", "QTIConverting", "temml.cjs");
_engine.Execute(File.ReadAllText(scriptPath));
_engine.Execute("var temml = module.exports;");
```
Can be can called in order to instruct the `Jint.Engine` to execute the desired script.

### Package

The `temmls.cjs` packages is manually copied within the `API/Tools/QTIConvering`. This package can not be automatically updated. In order to update this the new version of Temml should be copied within this folder.

The source code can be found here: https://github.com/ronkok/Temml.
The correct file can be found at `Temml/utils/temml.cjs`.