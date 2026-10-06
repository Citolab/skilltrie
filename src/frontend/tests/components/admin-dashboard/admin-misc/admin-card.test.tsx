/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import "@testing-library/jest-dom/vitest";
import { expect, describe, test } from "vitest";
import { render, screen, fireEvent } from "@testing-library/react";
import AdminCard from "../../../../src/components/admin-dashboard/admin-misc/admin-card";

describe("AdminCard", () => {
    function renderAdCard() {
        render(
            <AdminCard title="AdCard">
                <div>This is the child content</div>
            </AdminCard>
        );
    }
    test("Title visible", () => {
        renderAdCard();
        expect(screen.getByText(/AdCard/i)).toBeInTheDocument();
    });
    test("Child rendered", () => {
        renderAdCard();
        expect(screen.getByText(/child content/i)).toBeInTheDocument();
    });
    test("Child hidden", () => {
        renderAdCard();
        const child = screen.getByText(/child content/i);
        const parent = child.parentElement;
        expect(parent).toHaveClass("hidden");
    });
    test("Child visible after click", () => {
        renderAdCard();
        const child = screen.getByText(/child content/i);
        const parent = child.parentElement!.parentElement!;
        fireEvent.click(parent);
        expect(parent).not.toHaveClass("hidden");
        expect(child).toBeInTheDocument();
    });
});
