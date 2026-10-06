import * as fs from "fs";
import * as path from "path";

function loadTypeMap(paths: string[]): Map<string, string> {
    const text = paths.reduce((acc, v) => acc + fs.readFileSync(v, "utf-8"), "");

    const map = new Map<string, string>();

    const lines = text.split(/\r?\n/);

    for (const line of lines) {
        if (!line.trim()) continue;

        const parts = line.split("|");
        if (parts.length !== 3) continue;

        let [fullyQualifiedName, url, displayName] = parts.map(p => p.trim());

        if (!fullyQualifiedName.startsWith("T:")) continue;

        fullyQualifiedName = fullyQualifiedName.replace(/T:|`\d/g, "");
        displayName = displayName.replace(/<.+?>+/g, "");

        const fullyQualifiedEscapedName = fullyQualifiedName.replaceAll(".", "\\.");

        map.set(fullyQualifiedName, displayName);
        map.set(fullyQualifiedEscapedName, displayName);
    }

    return map;
}

function walkDirectory(dir: string): string[] {
    const results: string[] = [];
    const entries = fs.readdirSync(dir, { withFileTypes: true });

    for (const entry of entries) {
        const full = path.join(dir, entry.name);
        if (entry.isDirectory()) {
            results.push(...walkDirectory(full));
        } else {
            results.push(full);
        }
    }

    return results;
}

function rewriteContent(
    content: string,
    typeMap: Map<string, string>
): string {

    for (const [key, value] of typeMap) {
        content = content.replaceAll(key, value);
    }

    return content;
}

function rewriteh2toh1(content: string): string {
    return content.replace(/## (?=\w)/, "<br />\n\n# ");
}

/**
 * Escapes visible generic syntax:
 * `Func<TResult> -> Func&lt;TResult&gt;`
 */
function escapeVisibleGenerics(content: string): string {
    return content.replace(
        /`([^`]*<[^\n`>]+>[^`]*)`/g,
        (_, code) => {
            return "`"
                + code
                    .replace(/</g, "&lt;")
                    .replace(/>/g, "&gt;")
                + "`";
        }
    );
}

/**
 * URL-encodes markdown link fragments only.
 *
 * Example:
 * `(Run.md#AbTest.Run<TResult>)`
 * ->
 * `(Run.md#AbTest.Run%3CTResult%3E)`
 */
function encodeMarkdownAnchors(content: string): string {
    return content.replace(
        /\]\(([^)#]+)#([^)]+)\)/g,
        (_, file, anchor) => {
            const encodedAnchor = anchor
                .replace(/</g, "%3C")
                .replace(/>/g, "%3E")
                .replace(/ /g, "%20");

            return `](${file}#${encodedAnchor})`;
        }
    );
}

/**
 * Escapes raw HTML-like generics outside code blocks.
 *
 * Note: This regex wont always result in the correct result, but will make sure that the docs can at least be built
 *
 * Prevents Vue from interpreting:
 * `Action<FlagResult>`
 * as HTML tags.
 */
function escapeRawGenerics(content: string): string {
    return content.replace(
        /([A-Za-z0-9_.]+)<([^>\n]+)>/g,
        (_, left, inner) => {
            return `${left}&lt;${inner}&gt;`;
        }
    );
}

async function main() {
    const docsDir = process.argv[2];
    // Usage: node postprocess.mts docs/ mapping1.txt mapping2.txt mapping3.txt
    const mapFiles = process.argv.slice(3);

    if (!docsDir || !mapFiles) {
        console.error("Usage: node postprocess.mts [docsDir] [mapFile]");
        process.exit(1);
    }

    const typeMap = loadTypeMap(mapFiles);
    const files = walkDirectory(docsDir);

    for (const file of files) {
        if (!file.endsWith(".md")) continue;

        const original = fs.readFileSync(file, "utf8");
        let rewritten = rewriteContent(original, typeMap);

        rewritten = rewriteh2toh1(rewritten);

        rewritten = encodeMarkdownAnchors(rewritten);
        rewritten = escapeVisibleGenerics(rewritten);
        rewritten = escapeRawGenerics(rewritten);

        if (rewritten !== original) {
            console.log(`\x1b[32mProcessed: ${file}\x1b[0m`);
            fs.writeFileSync(file, rewritten, "utf8");
        }
    }
}

main();