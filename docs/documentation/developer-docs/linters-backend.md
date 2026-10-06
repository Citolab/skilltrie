# Backend Code Style and Formatting (C#)

The backend codebase follows consistent **C# coding conventions** enforced via an **`.editorconfig`** file.  
This ensures uniform formatting, naming, and style rules across all contributors and IDEs.

## Overview

| Tool | Purpose |
|------|----------|
| **.editorconfig** | Defines code formatting, naming, and style conventions enforced automatically by the IDE and the .NET SDK. |

## Configuration

### `.editorconfig`

The backend project includes a **`.editorconfig`** file located at the root of the project folder.  
This file defines global and language-specific rules for code style and formatting.


## Usage
Most modern IDEs (including Visual Studio, Vim and JetBrains Rider) automatically detect and apply `.editorconfig` settings.  
If you're using a different editor, check [editorconfig.org](https://editorconfig.org/) to confirm support and setup instructions.

You can also apply `.editorconfig` rules manually using the .NET CLI:

```bash
dotnet format
```

## Docs:
See Microsoft’s official documentation for .NET code style options:  
[Code Style Rules in .editorconfig (Microsoft Docs)](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/code-style-rule-options)