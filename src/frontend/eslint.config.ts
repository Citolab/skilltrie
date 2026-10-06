/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import globals from "globals";
import tseslint from "typescript-eslint";
import pluginReact from "eslint-plugin-react";
import reactHooks from "eslint-plugin-react-hooks";
import reactRefresh from "eslint-plugin-react-refresh";
import reactNamingConvention from "eslint-plugin-react-naming-convention";
import eslintPluginPrettier from "eslint-config-prettier";
import { Config, defineConfig } from "eslint/config";

export default defineConfig([
    {
        ignores: ["**/*.config.ts", "**/*.config.js", "**/coverage/**", "**/node_modules/**"],
    },
    {
        settings: {
            react: {
                version: "detect",
            },
        },
        files: ["**/*.{js,mjs,cjs,ts,mts,cts,jsx,tsx}"],
        extends: [
            tseslint.configs.recommendedTypeChecked,
            {
                languageOptions: {
                    parserOptions: { projectService: true },
                },
            },
            pluginReact.configs.flat.recommended,
            pluginReact.configs.flat["jsx-runtime"],
            reactHooks.configs["recommended-latest"],
            reactRefresh.configs.vite,
            reactNamingConvention.configs.recommended,
            eslintPluginPrettier, // Important, disables ESlint rules that conflict with Prettier
        ],

        languageOptions: { globals: globals.browser },
        rules: {
            "react/react-in-jsx-scope": "off",
            "react-hooks/rules-of-hooks": "error",
            "react-hooks/exhaustive-deps": "warn",
            "react-naming-convention/component-name": ["warn"],
            "react-naming-convention/context-name": ["warn"],
            "react-naming-convention/filename": ["warn", { rule: "kebab-case" }],
            "@/func-style": ["warn", "declaration"],
            eqeqeq: ["error", "always"],
            "no-var": "warn",
            // No need to add typing to obvious types
            "@typescript-eslint/no-inferrable-types": [
                "error",
                { ignoreParameters: true, ignoreProperties: true },
            ],

            "no-restricted-syntax": [
                "error",
                {
                    selector: "PropertyDefinition[key.type='PrivateIdentifier']",
                    message:
                        "Do not use private fields (#field). Use TypeScript's 'private' modifier instead.",
                },
                {
                    selector:
                        "AssignmentExpression[left.type='MemberExpression'][left.object.type='ThisExpression']",
                    message:
                        "Initialize class fields where they are declared instead of inside the constructor.",
                },
            ],

            "@typescript-eslint/naming-convention": [
                "warn",
                {
                    selector: "objectLiteralProperty",
                    modifiers: ["requiresQuotes"],
                    format: null,
                },
                {
                    selector: "function",
                    modifiers: ["exported"],
                    format: ["camelCase"],
                    filter: {
                        regex: "^use[A-Z].*",
                        match: true,
                    },
                },
                {
                    selector: [
                        "class",
                        "interface",
                        "typeAlias",
                        "enum",
                        "typeParameter",
                        "function",
                    ],
                    modifiers: ["exported"],
                    format: ["PascalCase"],
                },
                {
                    selector: "variable",
                    types: ["function"],
                    modifiers: ["exported"],
                    format: ["PascalCase"],
                    filter: {
                        regex: "^(use|set|[A-Z])",
                        match: false,
                    },
                },
                {
                    selector: "property",
                    format: ["camelCase", "UPPER_CASE", "PascalCase"],
                    leadingUnderscore: "allowDouble",
                },
                {
                    selector: ["variable", "parameter", "function", "method", "property"],
                    format: ["camelCase"],
                },
                {
                    selector: ["enumMember"],
                    format: ["UPPER_CASE"],
                },
            ],
        },
    },
]);
