/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { render, screen } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { describe, it, expect, vi } from "vitest";
import "@testing-library/jest-dom/vitest";

import GroupPage from "../../../src/pages/admin/dashboards/group-page";

// --- Mock API calls ---
vi.mock("../../../src/api/group", () => ({
    GetGroup: vi.fn().mockResolvedValue({
        id: 10,
        name: "Test Group",
        members: [
            { user: { id: 1, firstName: "A", lastName: "B" } },
            { user: { id: 2, firstName: "C", lastName: "D" } },
        ],
    }),
    GetGroupProficiency: vi.fn().mockResolvedValue({
        averageProficiency: 75,
    }),
    AddGroupMember: vi.fn().mockResolvedValue({}),
    RemoveGroupMember: vi.fn().mockResolvedValue({}),
}));

// --- Mock heavy child components ---
vi.mock("../../../src/components/admin-dashboard/admin-table/admin-table.tsx", () => ({
    default: () => <div data-testid="mock-admin-table" />,
}));

vi.mock("../../../src/components/admin-dashboard/admin-popup-window.tsx", () => ({
    default: ({ isOpen }: any) =>
        isOpen ? <div data-testid="mock-popup-window" /> : null,
}));

vi.mock("../../../src/components/admin-dashboard/admin-generic-object-form.tsx", () => ({
    default: () => <div data-testid="mock-object-form" />,
}));

vi.mock("../../../src/components/admin-dashboard/admin-input/admin-button.tsx", () => ({
    default: ({ children, onClick }: any) => (
        <button data-testid="mock-admin-button" onClick={onClick}>
            {children}
        </button>
    ),
}));

describe("GroupPage", () => {
    const renderWithRoute = () =>
        render(
            <MemoryRouter initialEntries={["/admin/group/10"]}>
                <Routes>
                    <Route path="/admin/group/:id" element={<GroupPage />} />
                </Routes>
            </MemoryRouter>
        );

    it("renders the group page after loading", async () => {
        renderWithRoute();

        // Loading state appears first
        expect(screen.getByText(/loading/i)).toBeInTheDocument();

        // Then the group name appears
        expect(await screen.findByText("Test Group")).toBeInTheDocument();
    });

    it("renders the members table", async () => {
        renderWithRoute();

        expect(await screen.findByTestId("mock-admin-table")).toBeInTheDocument();
    });

    it("renders the proficiency section", async () => {
        renderWithRoute();

        expect(await screen.findByText(/average: 75/i)).toBeInTheDocument();
    });

    it("opens the Add Member popup when button is clicked", async () => {
        renderWithRoute();

        const button = await screen.findByTestId("mock-admin-button");
        button.click();

        expect(await screen.findByTestId("mock-popup-window")).toBeInTheDocument();
    });
});