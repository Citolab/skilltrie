# Frontend Linting and Formatting

This project uses **ESLint** and **Prettier** to ensure consistent code style and catch potential issues early.  
All contributors should have these tools properly configured before committing any code.

## Overview

| Tool | Purpose |
|------|----------|
| **ESLint** | Identifies and reports on problematic JavaScript/TypeScript code. This includes naming conventions. |
| **Prettier** | Automatically formats code for consistency and readability. |

## Installation

After cloning the repository, install dependencies (this will include ESLint and Prettier):

```bash
npm install
```

If you want to install them manually run:

```bash
npm install --save-dev eslint prettier
```

## Configuration

### ESLint

Eslint is configured via the `eslint.config.ts` file located in the frontend folder of the project.
Refer to the documentation links for each plugin for more details about available rules, options, and usage.

**Plugin Docs:** 
- [TypeScript ESLint](https://typescript-eslint.io/getting-started/)
- [ESLint React Plugin](https://www.npmjs.com/package/eslint-plugin-react)
- [React Hooks Plugin](https://www.npmjs.com/package/eslint-plugin-react-hooks)
- [React Refresh Plugin](https://www.npmjs.com/package/eslint-plugin-react-refresh)
- [React Naming Convention Plugin](https://www.npmjs.com/package/eslint-plugin-react-naming-convention)

### Prettier

Prettier is configured via the `.prettierrc` file located in the frontend folder of the project.  
**Docs:** [Prettier Configuration Options](https://prettier.io/docs/options)

## Usage

### Run ESLint Manually

```bash
# Lint all files
npm run lint

# Lint a specific file
npm run lint [filepath]

# Automatically (partially) fix issues.
# Note: ESLint can't fix every type of issue automatically.
npm run lint [filepath] -- --fix
```

### Run Prettier Manually

This will **not** affect code functionality, just formatting.

```bash
# Format all files
npm run format

# Format a specific file
npm run format [filepath]
```

## VS Code Setup (Recommended)

To make linting and formatting automatic in **Visual Studio Code**, install these extensions:

| Extension                     | Link                                                                                             | Purpose                                                |
| ----------------------------- | ------------------------------------------------------------------------------------------------ | ------------------------------------------------------ |
| **ESLint**                    | [ESLint Extension](https://marketplace.visualstudio.com/items?itemName=dbaeumer.vscode-eslint)   | Highlights and auto-fixes linting issues in real time. |
| **Prettier – Code Formatter** | [Prettier Extension](https://marketplace.visualstudio.com/items?itemName=esbenp.prettier-vscode) | Automatically formats your code on save.               |

### VS Code Settings

After installing the extensions above, add the following to your VS code `settings.json` file. This can be done by following these steps:

1. Open **Command Palette** (`Ctrl + Shift + P)
2. Type `Preference: Open Settings (JSON)` and hit Enter
3. Paste the following snippet into your settings file

```bash
{
  "[javascript][typescript][typescriptreact][javascriptreact]": {
    "editor.defaultFormatter": "esbenp.prettier-vscode",
  },
  "[csharp]": {
    "editor.defaultFormatter": "ms-dotnettools.csharp"
  },
  "editor.formatOnType": false,
  "editor.formatOnPaste": true,
  "editor.formatOnSave": true,
  "editor.formatOnSaveMode": "file",
  "files.autoSave": "onFocusChange"
}

```
