/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import "@testing-library/jest-dom/vitest";
import { vi, expect, describe, test } from "vitest";
import { render, screen, fireEvent } from "@testing-library/react";
import AdminToggleButton from "../../../../src/components/admin-dashboard/admin-input/admin-toggle-button";

describe("Toggle Button", () => {
    const mockOnChange = vi.fn();
    const states = ["Tic", "Tac", "Toe"];
    function renderToggle() {
        render(
            <AdminToggleButton
                states={states}
                selectedState={states[0]}
                stateSelected={mockOnChange}
            />
        );
    }
    test("Initial state shown", () => {
        renderToggle();
        expect(screen.getByText("Tic")).toBeInTheDocument();
    });

    test("Clicked state", () => {
        renderToggle();
        const button = screen.getByRole("button");
        fireEvent.click(button);
        expect(mockOnChange).toHaveBeenCalledWith("Tac");
    });
});
