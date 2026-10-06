/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { afterEach, vi } from "vitest";
import { cleanup } from "@testing-library/react";

afterEach(() => {
    cleanup();
});

Object.defineProperty(window, "matchMedia", {
    writable: true,
    value: vi.fn().mockImplementation((query) => ({
        matches: false,
        // eslint-disable-next-line @typescript-eslint/no-unsafe-assignment
        media: query,
        onchange: null,
        addListener: vi.fn(), // deprecated
        removeListener: vi.fn(), // deprecated
        addEventListener: vi.fn(),
        removeEventListener: vi.fn(),
        dispatchEvent: vi.fn(),
    })),
});

// tests/setup.ts
if (typeof CSSStyleSheet.prototype.replaceSync === 'undefined') {
  CSSStyleSheet.prototype.replaceSync = function (css: string) {
    // no-op or minimal stub — JSDOM doesn't render styles anyway
  };
}

// Also polyfill adoptedStyleSheets if needed
if (!Object.getOwnPropertyDescriptor(Document.prototype, 'adoptedStyleSheets')) {
  Object.defineProperty(Document.prototype, 'adoptedStyleSheets', {
    get() { return []; },
    set() {},
  });
}

if (!Object.getOwnPropertyDescriptor(ShadowRoot.prototype, 'adoptedStyleSheets')) {
  Object.defineProperty(ShadowRoot.prototype, 'adoptedStyleSheets', {
    get() { return []; },
    set() {},
  });
}