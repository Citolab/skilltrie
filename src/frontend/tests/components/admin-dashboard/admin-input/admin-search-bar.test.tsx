/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import "@testing-library/jest-dom/vitest";
import { vi, expect, describe, test, beforeEach } from "vitest";
import { render, screen, fireEvent } from "@testing-library/react";
import AdminSearchBar from "../../../../src/components/admin-dashboard/admin-input/admin-search-bar";

describe("Search bar", () => {
    beforeEach(() => {
        vi.restoreAllMocks();
    });
    const mockOnChange = vi.fn();
    function renderSearchBar() {
        render(
            <AdminSearchBar
                label="Search"
                placeholder="Enter search string"
                initialValue="Initial search"
                onChange={mockOnChange}
            />
        );
    }

    test("Label visible", () => {
        renderSearchBar();
        expect(screen.getByText(/Search/i)).toBeInTheDocument();
    });

    test("Placeholder visible", () => {
        renderSearchBar();
        expect(screen.getByPlaceholderText(/Enter search string/i)).toBeInTheDocument();
    });

    test("Initial value visible", () => {
        renderSearchBar();
        expect(screen.getByDisplayValue(/Initial search/i)).toBeInTheDocument();
    });

    test("Search working with initial value", () => {
        renderSearchBar();
        const button = screen.getByRole("button");
        fireEvent.click(button);
        expect(mockOnChange).toHaveBeenCalledWith("Initial search");
    });

    test("Search working with changed value", () => {
        renderSearchBar();
        screen.debug();
        const button = screen.getByRole("button");
        const input = screen.getByRole("textbox");
        fireEvent.change(input, { target: { value: "Updated search" } });
        fireEvent.click(button);
        expect(mockOnChange).toHaveBeenCalledWith("Updated search");
    });
});
