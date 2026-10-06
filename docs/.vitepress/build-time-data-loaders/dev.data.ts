/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

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
  "../documentation/developer-docs/**/*.md",
  {
    transform(data) {
      return data.map(d => {
        const segments = d.url?.split("/");
        let title = segments?.[segments?.length - 1];
        if (title === "") {
          title = segments?.[segments?.length - 2];
        }
        title = title.replace(/-/g, " ").replace(/\b\w/g, c => c.toUpperCase());
        
        // Prepend base path
        const url = base + d.url;
        
        return { ...d, url:url, title: title ?? d.src }
      }).filter(d => d.title !== "Developer Docs");
    },
  }
);