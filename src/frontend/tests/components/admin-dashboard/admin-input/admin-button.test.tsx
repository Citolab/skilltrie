/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { useState } from "react";
import "@testing-library/jest-dom/vitest";
import { expect, describe, test } from "vitest";
import { render, screen, fireEvent } from "@testing-library/react";
import AdminButton from "../../../../src/components/admin-dashboard/admin-input/admin-button.tsx";

export function EnabledButton() {
    const [enabledText, setEnabledText] = useState<string>("Hello");
    return <AdminButton onClick={() => setEnabledText("Bye")}>{enabledText}</AdminButton>;
}
export function DisabledButton() {
    const [disabledText, setDisabledText] = useState<string>("Yellow");
    return (
        <AdminButton disabled={true} onClick={() => setDisabledText("Blue")}>
            {disabledText}
        </AdminButton>
    );
}

describe("Component rendering", () => {
    test("Enabled visible", () => {
        render(<EnabledButton />);
        expect(screen.getByText("Hello")).toBeInTheDocument();
    });

    test("Disabled visible", () => {
        render(<DisabledButton />);
        expect(screen.getByText("Yellow")).toBeInTheDocument();
    });
});

describe("Disabled and onClick properties", () => {
    test("Enabled button functional", () => {
        render(<EnabledButton />);

        const button = screen.getByRole("button", { name: /Hello/i });
        fireEvent.click(button);

        expect(screen.getByRole("button", { name: /Bye/i }));
    });

    test("Disabled button does NOT react to clicks", () => {
        render(<DisabledButton />);
        const button = screen.getByRole("button", { name: /Yellow/i });
        fireEvent.click(button);

        expect(screen.getByRole("button", { name: /Yellow/i })).toBeInTheDocument();
    });
});

screen.debug();
