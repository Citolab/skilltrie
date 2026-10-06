/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { defineConfig, loadEnv } from "vite";
import react from "@vitejs/plugin-react";
import path from "node:path";
import tailwindcss from "@tailwindcss/vite";
import { version } from './package.json';
import svgr from "vite-plugin-svgr";

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, '../', '');

  return {
    envDir: '../',
    base: env.VITE_BASE_PATH || '/',
    plugins: [react(), tailwindcss(), svgr()],
    server: {
      proxy: {
        "^/api/.*": {
          target: env.VITE_BACKEND_URL,
        },
      },
      allowedHosts: [
        env.VITE_SERVER_DOMAIN,
        env.VITE_SERVER2_DOMAIN
      ].filter(Boolean) as string[],
    },
    build: {
      outDir: env.VITE_OUT_DIR || path.resolve(__dirname, "../backend/wwwroot"),
      emptyOutDir: true,
    },
    resolve: {
      alias: {
        "@": path.resolve(__dirname, "src"),
      },
    },
    define: {
      __APP_VERSION__: JSON.stringify(version),
    },
  };
});
