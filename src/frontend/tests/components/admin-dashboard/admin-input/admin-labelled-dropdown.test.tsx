/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import "@testing-library/jest-dom/vitest";
import { vi, expect, describe, test } from "vitest";
import { render, screen, fireEvent } from "@testing-library/react";
import AdminLabelledDropdown from "../../../../src/components/admin-dashboard/admin-input/admin-labelled-dropdown.tsx";

type Fruit = {
    name: string;
    colour: string;
    age: number;
};
const fruitShape: Fruit = {
    name: "",
    colour: "",
    age: 0,
};

const options = Object.keys(fruitShape) as (keyof Fruit)[];

describe("With optionNames", () => {
    const mockOnChange = vi.fn();
    function renderWithNames() {
        render(
            <AdminLabelledDropdown<Fruit>
                label="Hello"
                options={options}
                optionNames={{
                    name: "Fruit Name",
                    colour: "Fruit Colour",
                    age: "Fruit Age",
                }}
                defaultOption="name"
                onChange={mockOnChange}
            />
        );
    }
    test("Renders the label", () => {
        renderWithNames();
        expect(screen.getByText("Hello")).toBeInTheDocument();
    });
    test("Render the options", () => {
        renderWithNames();
        expect(screen.getByText("Fruit Name")).toBeInTheDocument();
        expect(screen.getByText("Fruit Colour")).toBeInTheDocument();
        expect(screen.getByText("Fruit Age")).toBeInTheDocument();
    });
    test("onChange working", () => {
        renderWithNames();
        const select = screen.getByRole("combobox");
        fireEvent.change(select, { target: { value: "Fruit Colour" } });
        expect(mockOnChange).toHaveBeenCalledTimes(1);
        expect(mockOnChange).toHaveBeenCalledWith("colour");
    });
});

describe("without optionNames", () => {
    const mockOnChange = vi.fn();
    function renderWithoutNames() {
        render(
            <AdminLabelledDropdown<Fruit>
                label="Hello"
                options={options}
                defaultOption="name"
                onChange={mockOnChange}
            />
        );
    }

    test("Renders the label", () => {
        renderWithoutNames();
        expect(screen.getByText("Hello")).toBeInTheDocument();
    });
    test("Render the options", () => {
        renderWithoutNames();
        expect(screen.getByText("name")).toBeInTheDocument();
        expect(screen.getByText("colour")).toBeInTheDocument();
        expect(screen.getByText("age")).toBeInTheDocument();
    });
    test("onChange working", () => {
        renderWithoutNames();
        const select = screen.getByRole("combobox");
        fireEvent.change(select, { target: { value: "colour" } });
        expect(mockOnChange).toHaveBeenCalledTimes(1);
        expect(mockOnChange).toHaveBeenCalledWith("colour");
    });
});

screen.debug();
