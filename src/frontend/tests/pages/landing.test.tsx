/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { expect, describe, it, beforeEach } from "vitest";
import { render, screen } from "@testing-library/react";
import "@testing-library/jest-dom/vitest";
import { MemoryRouter } from "react-router-dom";
import LaunchPage from "../../src/pages/landing";

describe("LaunchPage", () => {
    beforeEach(() => {
        render(
            <MemoryRouter>
                <LaunchPage />
            </MemoryRouter>
        );
    });

    it("renders the brand name", () => {
        expect(screen.getByText("SKILLTRIE")).toBeInTheDocument();
    });

    it("renders the description paragraph", () => {
        expect(
            screen.getByText(/build intuition through interactive exercises/i)
        ).toBeInTheDocument();
    });

    it("renders the login and register links in the header", () => {
        expect(screen.getByRole("link", { name: /log in/i })).toHaveAttribute("href", "/login");
        expect(screen.getByRole("button", { name: /register/i })).toBeInTheDocument();
    });

    it('renders the "Start Learning" call-to-action button', () => {
        const cta = screen.getByRole("button", { name: /start learning/i });
        expect(cta).toBeInTheDocument();
    });

    it("renders the footer content", () => {
        expect(
            screen.getByText(/A web-app built by Stastiftics and continued by SkillTrie/i)
        ).toBeInTheDocument();
    });
});
