/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import "@testing-library/jest-dom/vitest";
import { vi, expect, describe, test, beforeEach } from "vitest";
import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import AdminSearchAndPagination from "../../../src/components/admin-dashboard/admin-search-and-pagination";
import { MemoryRouter } from "react-router-dom";

const pagination = vi.fn().mockResolvedValue([]);
const search = vi.fn().mockResolvedValue([]);
const getObjects = vi.fn();

function renderSearchPage() {
    render(
        <MemoryRouter>
            <AdminSearchAndPagination
                sortOptions={["id", "name", "email"]}
                sortOptionNames={{
                    id: "User ID",
                    name: "Full Name",
                    email: "Email Address",
                }}
                searchPlaceholder="Search users..."
                endpoints={{ pagination, search }}
                getObjects={getObjects}
                refresh={() => {}}
            />
        </MemoryRouter>
    );
}
describe("Pagination", () => {
    beforeEach(() => {
        pagination.mockClear();
    });
    test("Called on mount", () => {
        renderSearchPage();
        expect(pagination).toHaveBeenCalledWith(0, 15, "id", "ascending");
    });

    test("Next button fetches next page", () => {
        renderSearchPage();
        const next = screen.getByRole("button", { name: /next/i });
        fireEvent.click(next);

        expect(pagination).toHaveBeenCalledWith(15, 15, "id", "ascending");
    });

    test("Next button disabled at end of pagination", async () => {
        pagination.mockResolvedValueOnce([
            { id: 1, name: "A" },
            { id: 2, name: "B" },
            { id: 3, name: "C" },
        ]);

        renderSearchPage();
        await screen.findByText(/sort on/i);
        const next = screen.getByRole("button", { name: /next/i });

        expect(next).toBeDisabled();
    });

    test("Previous button fetches next page", () => {
        renderSearchPage();
        const next = screen.getByRole("button", { name: /next/i });
        fireEvent.click(next);

        const prev = screen.getByRole("button", { name: /previous/i });
        fireEvent.click(prev);

        expect(pagination).toHaveBeenCalledWith(0, 15, "id", "ascending");
    });

    test("Previous button disabled on 0 offset", () => {
        renderSearchPage();
        const prev = screen.getByRole("button", { name: /previous/i });
        expect(prev).toBeDisabled();
    });
    test("Range adjustment calls pagination", () => {
        renderSearchPage();
        const rangeLabel = screen.getByText("Range");
        const select = rangeLabel.parentElement!.querySelector("select")!;
        fireEvent.change(select, { target: { value: "20" } });

        expect(pagination).toHaveBeenLastCalledWith(0, 20, "id", "ascending");
    });
});

describe("Sorting", () => {
    test("Changing sort column resorts objects", async () => {
        renderSearchPage();
        getObjects.mockClear();
        const sortLabel = screen.getByText("Sort on");
        const select = sortLabel.parentElement!.querySelector("select")!;
        fireEvent.change(select, { target: { value: "name" } });
        await waitFor(() => expect(getObjects).toHaveBeenCalledOnce());
    });

    test("Changing sort order resorts objects", async () => {
        renderSearchPage();
        getObjects.mockClear();
        const button = screen.getByRole("button", { name: /sort by:/i });
        fireEvent.click(button);
        await waitFor(() => expect(getObjects).toHaveBeenCalledOnce());
    });
});

describe("Search", () => {
    beforeEach(() => {
        search.mockClear();
    });

    test("Triggers when query has >=3 chars", () => {
        renderSearchPage();
        const button = screen.getByRole("button", { name: /search/i });
        const input = screen.getByPlaceholderText(/Search users.../);
        fireEvent.change(input, { target: { value: "abc" } });
        fireEvent.click(button);
        expect(search).toHaveBeenLastCalledWith("abc");
    });
    test("Doesn't trigger when query has <3 chars", () => {
        renderSearchPage();
        const button = screen.getByRole("button", { name: /search/i });
        const input = screen.getByPlaceholderText(/Search users.../);
        fireEvent.change(input, { target: { value: "ab" } });
        fireEvent.click(button);
        expect(search).not.toHaveBeenCalledWith("ab");
    });
});

// TODO: More tests should be written here on pagination + search interaction
// Not doing this now as this is not implemented
