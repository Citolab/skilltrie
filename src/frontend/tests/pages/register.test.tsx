/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { describe, it, expect } from "vitest";
import { render, screen } from "@testing-library/react";
import { BrowserRouter } from "react-router-dom";
import Register from "../../src/pages/register";
import "@testing-library/jest-dom/vitest";

describe("Register page", () => {
    it("renders all input fields and buttons", () => {
        render(
            <BrowserRouter>
                <Register />
            </BrowserRouter>
        );
        expect(screen.getByRole("button", { name: /Register/i })).toBeInTheDocument();
        expect(screen.getByRole("checkbox", { name: /privacy statement/i })).toBeInTheDocument();
        expect(screen.getByText("Log in here")).toBeInTheDocument();
    });
});
