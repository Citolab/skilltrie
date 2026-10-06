import { createContentLoader } from "vitepress";

/**
 *  These files are used to create 'index' pages for frontend, backend, etc.
 *  The createContentLoader is an API of VitePress designed specifically for this purpose.
 *  Documentation here: https://vitepress.dev/guide/data-loading#createcontentloader
 * 
 *  The code in this file is executed once at build time, even in dev mode. 
 *  HMR will thus not affect this file -> restart the dev-server if you want to see
 *  your changes reflected!
 */
const base = process.env.VITE_BASE_PATH?.replace(/\/$/, "") ?? "/docs";

export default createContentLoader(
    "../documentation/handover/**/*.md",
    {
        transform(data) {
            return data.map(d => {
                const segments = d.url?.split("/");

                let title = segments?.[segments?.length - 1];

                if (title === "") {
                    title = segments?.[segments?.length - 2];
                }
                const url = base + d.url;
                title = title.replace(/-/g, " ").replace(/\b\w/g, c => c.toUpperCase());

                return { ...d, title: title ?? d.src , url:url}
            }).filter(d => d.title !== "Handover"); // prevent the index page from listing itself.
        },
    }
);