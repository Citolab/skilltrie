/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { StrictMode } from "react";
import { createRoot } from "react-dom/client";

import "./index.css";
import App from "./app.tsx";
/* QTI related */
import type { CustomElements } from "@citolab/qti-components/react";
import "@citolab/qti-components";
import { PostHogProvider } from "posthog-js/react";
import posthog from "posthog-js";

declare const __APP_VERSION__: string;

/* TypeScript boilerplate for linter support for qti components */
declare module "react" {
    // eslint-disable-next-line @typescript-eslint/no-namespace
    namespace JSX {
        interface IntrinsicElements extends CustomElements {
            style: React.DetailedHTMLProps<
                React.StyleHTMLAttributes<HTMLStyleElement>,
                HTMLStyleElement
            >;
        }
    }
}

// Register posthog
posthog.init(import.meta.env.VITE_POSTHOG_PROJECT_TOKEN, {
    api_host: import.meta.env.VITE_POSTHOG_HOST,
    ui_host: import.meta.env.VITE_POSTHOG_UI_HOST,
    defaults: "2026-01-30",
    capture_exceptions: {
        capture_unhandled_errors: true,
        capture_unhandled_rejections: true,
        capture_console_errors: false,
    },
});

posthog.register({
    version: __APP_VERSION__,
}) // Always pass the app version as parameter when capturing events. 

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <PostHogProvider client={posthog}>
      <App />
    </PostHogProvider>
  </StrictMode>
)
