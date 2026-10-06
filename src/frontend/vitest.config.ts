/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { defineConfig } from "vitest/config";
import { version } from './package.json';

export default defineConfig({
  test: {
    environment: "jsdom",
    setupFiles: "/tests/setup.ts",
  },
  define: {
      __APP_VERSION__: JSON.stringify(version),
  },
});
