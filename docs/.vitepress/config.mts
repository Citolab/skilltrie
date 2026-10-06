import { defineConfig, UserConfig } from "vitepress";
import { withSidebar } from "vitepress-sidebar";
import { VitePressSidebarOptions } from "vitepress-sidebar/types";

// https://vitepress.dev/reference/site-config
const vitePressConfig: Partial<UserConfig> = {
    srcDir: "documentation",
    base: "/docs/",
    outDir: "public",

    title: "SkillTrie Docs",

    head: [["link", { rel: "icon", type: "image/svg+xmp", href: "/skilltrie_logo.svg"}]],

    themeConfig: {
        // https://vitepress.dev/reference/default-theme-config
        siteTitle: "SkillTrie Docs",
        nav: [
            { text: "Home",             link: "/"               },
            { text: "Handover",         link: "/handover"       },
            { text: "Frontend",         link: "/frontend"       },
            { text: "Backend",          link: "/backend"        },
            { text: "Developer Docs",   link: "/developer-docs" },
            { text: "Auto Docs",        link: "/auto-docs"      }
        ],
        socialLinks: [
            {
                icon: "gitlab",
                link: "https://gitlab.science.uu.nl/ics/sp/2026/v26d/cito-software-project",
            },
        ],
        search: {
            provider: "local",
        },
    },

    rewrites: {
        "software-project-cito/:page*": ":page*",
    },

    ignoreDeadLinks: true,
    cleanUrls: true,
    lastUpdated: true,
};

const vitePressSidebarConfigs: VitePressSidebarOptions[] = [
    {
        documentRootPath: "/documentation",
        scanStartPath: "auto-docs/",
        basePath: "/auto-docs/",
        resolvePath: "/auto-docs/",
        collapseDepth: 1,
        useFolderLinkFromIndexFile: true,
        excludeByGlobPattern: ["Migrations/"],
    },
    {
        documentRootPath: "/documentation",
        scanStartPath: "software-project-cito/",
        basePath: "/",
        resolvePath: "/backend/",
        useFolderLinkFromIndexFile: true,
        capitalizeEachWords: true,
        hyphenToSpace: true,
    },
    {
        documentRootPath: "/documentation",
        scanStartPath: "software-project-cito/",
        basePath: "/",
        resolvePath: "/frontend/",
        useFolderLinkFromIndexFile: true,
        capitalizeEachWords: true,
        hyphenToSpace: true,
    },
    {
        documentRootPath: "/documentation",
        scanStartPath: "developer-docs",
        basePath: "/developer-docs/",
        resolvePath: "/developer-docs/",
        capitalizeEachWords: true,
        hyphenToSpace: true,
    },
    {
        documentRootPath: "/documentation",
        scanStartPath: "handover",
        basePath: "/handover/",
        resolvePath: "/handover/",
        capitalizeEachWords: true,
        hyphenToSpace: true,
    }
];

export default defineConfig(
    withSidebar(vitePressConfig, vitePressSidebarConfigs),
);

