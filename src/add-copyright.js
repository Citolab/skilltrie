/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

// This script can be run with
// `node add-copyright.js`
// or
// `node add-copyright.js X`
// where X is the maximum number of files to edit.
// With no X specified, it will change up to 100 files (specified on line 45).

const fs = require("fs");
const path = require("path");

const ROOT_DIR = process.cwd();

// Files to process
const EXTENSIONS = [".cs", ".ts", ".tsx"];

// Directories to skip
const IGNORE_DIRS = new Set([
    "node_modules",
    "bin",
    "obj",
    ".git",
    "dist",
    "build",
    "Migrations",
]);

// Copyright header
const COPYRIGHT_HEADER = `/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */
`;

// Marker to detect existing header.
const HEADER_MARKER = "© Copyright Utrecht University";

// For testing/debugging this script
const MAX_CHANGES = Number(process.argv[2]) || 100;
let changes = 0;

// This is needed to filter out `U+feff`,
//  which is a 0 space invisible character we apparently have in front of some using or import statements.
function stripBom(text) {
    return text.charCodeAt(0) === 0xfeff ? text.slice(1) : text;
}

function processDirectory(dir) {
    const entries = fs.readdirSync(dir, { withFileTypes: true });

    for (const entry of entries) {
        // For testing/debugging this script
        if (changes >= MAX_CHANGES) return;

        const fullPath = path.join(dir, entry.name);

        if (entry.isDirectory()) {
            if (!IGNORE_DIRS.has(entry.name)) {
                processDirectory(fullPath);
            }
            continue;
        }

        if (!EXTENSIONS.includes(path.extname(entry.name))) {
            continue;
        }

        let content = fs.readFileSync(fullPath, "utf8");
        content = stripBom(content);

        // Skip if header already present
        if (content.includes(HEADER_MARKER)) {
            continue;
        }

        fs.writeFileSync(fullPath, COPYRIGHT_HEADER + "\n" + content, "utf8");

        // For testing/debugging this script
        changes++;

        //console.log(`Updated: ${fullPath}`);
    }
}

processDirectory(ROOT_DIR);
console.log(`Done updating '${changes}' files.`);
