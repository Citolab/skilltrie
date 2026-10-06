/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import "@testing-library/jest-dom/vitest";
import { expect, describe, test } from "vitest";
import { render, screen } from "@testing-library/react";
import AdminTableHeader from "../../../../src/components/admin-dashboard/admin-table/admin-table-header";

describe("Admin table header", () => {
    function renderHeader() {
        render(
            <AdminTableHeader
                headers={["blue", "orange", "yellow"]}
                optionalHeaders={["blue", "black"]}
            />
        );
    }

    test("Headers visible", () => {
        renderHeader();
        const orange = screen.getByText(/orange/i);
        const yellow = screen.getByText(/yellow/i);

        expect(orange).toBeInTheDocument();
        expect(orange).not.toHaveClass("hidden");

        expect(yellow).toBeInTheDocument();
        expect(yellow).not.toHaveClass("hidden");
    });

    test("Included optional headers invisible", () => {
        renderHeader();
        const blue = screen.getByText(/blue/i);

        expect(blue).toBeInTheDocument();
        expect(blue).toHaveClass("hidden");
    });

    test("Excluded optional headers absent", () => {
        renderHeader();

        const black = screen.queryByText(/black/i);
        expect(black).not.toBeInTheDocument();
    });
});
