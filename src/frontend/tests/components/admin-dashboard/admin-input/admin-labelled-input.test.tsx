/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import "@testing-library/jest-dom/vitest";
import { vi, expect, describe, test, beforeEach } from "vitest";
import { render, screen, fireEvent } from "@testing-library/react";
import LabelledInput from "../../../../src/components/admin-dashboard/admin-input/admin-labelled-input";

describe("array Input", () => {
    const mockOnChange = vi.fn();
    function renderArray(disabledInput: boolean = false) {
        render(
            <LabelledInput
                type="array"
                label="ArrayInput"
                onChange={mockOnChange}
                disabled={disabledInput}
                placeholder="Enter text"
            />
        );
    }
    test("Placeholder visible", () => {
        renderArray();
        expect(screen.getByPlaceholderText(/Enter text/i)).toBeInTheDocument();
    });
    test("Onchange splitting functional", () => {
        renderArray();
        const input = screen.getByPlaceholderText(/Enter text/i);
        fireEvent.change(input, { target: { value: "tic,tac,toe" } });
        expect(mockOnChange).toBeCalledWith(["tic", "tac", "toe"]);
    });
});

describe("Boolean input", () => {
    function renderBoolean(disabledInput: boolean = false) {
        const mockOnChange = vi.fn();
        render(
            <LabelledInput
                type="boolean"
                label="boolinput"
                placeholder="Boolean input!"
                value="valueholder"
                errors={["Button on"]}
                disabled={disabledInput}
                onChange={mockOnChange}
            />
        );
    }

    test("Placeholder visible", () => {
        renderBoolean();
        expect(screen.getByText(/Boolean input!/i)).toBeInTheDocument();
    });

    test("Value not visible", () => {
        renderBoolean();
        expect(screen.queryByText(/valueholder/i)).not.toBeInTheDocument();
    });
});
describe("dropdown Input", () => {
    function renderDropdown(disabledInput: boolean = false) {
        const mockOnChange = vi.fn();
        render(
            <LabelledInput
                type="dropdown"
                label="Dropdown input"
                placeholder="Dropdown holder!"
                options={["1", "2"]}
                disabled={disabledInput}
                onChange={mockOnChange}
            />
        );
    }
    test("Values shown", () => {
        renderDropdown();
        expect(screen.queryByDisplayValue(/Dropdown holder/i)).not.toBeInTheDocument();
        expect(screen.getByText("1")).toBeInTheDocument();
        expect(screen.getByText("2")).toBeInTheDocument();
    });
});
describe("immutable Input", () => {
    function renderImmutable() {
        render(<LabelledInput type="immutable" label="Immutableinput" value="Immutable input!" />);
    }

    test("Value shown", () => {
        renderImmutable();
        expect(screen.getByDisplayValue(/Immutable input!/i)).toBeInTheDocument();
    });
});

describe("Number input", () => {
    const mockOnChange = vi.fn();
    beforeEach(() => {
        mockOnChange.mockClear();
    });
    function renderNumber(disabledInput: boolean = false) {
        render(
            <LabelledInput
                type="number"
                label="NumberLabel!"
                placeholder="Enter number"
                value={10}
                disabled={disabledInput}
                onChange={mockOnChange}
            />
        );
    }

    test("Placeholder shown", () => {
        renderNumber();
        expect(screen.getByPlaceholderText(/Enter Number/i)).toBeInTheDocument();
    });
    test("Value shown", () => {
        renderNumber();
        expect(screen.getByDisplayValue(/10/i)).toBeInTheDocument();
    });
    test("Values changed and ", () => {
        renderNumber();
        const input = screen.getByRole("spinbutton");
        fireEvent.change(input, { target: { value: "11" } });
        expect(mockOnChange).toHaveBeenCalledTimes(2);
    });
});
describe("Small-text Input", () => {
    const mockOnChange = vi.fn();
    function renderSmallText(disabledInput: boolean = false) {
        render(
            <LabelledInput
                type="small-text"
                label="TextLabel!"
                placeholder="Enter text"
                disabled={disabledInput}
                onChange={mockOnChange}
            />
        );
    }

    test("Placeholder visible", () => {
        renderSmallText();
        expect(screen.getByPlaceholderText(/Enter text/i)).toBeInTheDocument();
    });
});
describe("Text Input", () => {
    const mockOnChange = vi.fn();
    function renderText(disabledInput: boolean = false) {
        render(
            <LabelledInput
                type="text"
                label="TextLabel!"
                placeholder="Enter text"
                disabled={disabledInput}
                onChange={mockOnChange}
            />
        );
    }

    test("Placeholder visible", () => {
        renderText();
        expect(screen.getByPlaceholderText(/Enter text/i)).toBeInTheDocument();
    });
});

describe("Labels and errors on all inputs", () => {
    function renderLabelErrors() {
        render(
            <LabelledInput
                type="boolean"
                label="boolinput"
                placeholder="Boolean input!"
                value="valueholder"
                errors={["Button on", "Check"]}
            />
        );
    }
    test("Label visible", () => {
        renderLabelErrors();
        expect(screen.getByText(/boolinput/i)).toBeInTheDocument();
    });
});
