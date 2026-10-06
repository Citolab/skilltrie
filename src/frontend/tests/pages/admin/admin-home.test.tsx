/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */
import { expect, describe, it, beforeEach, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import "@testing-library/jest-dom/vitest";
import AdminHome from "../../../src/pages/admin/admin-home";

describe("Admin Home page", () => {
    beforeEach(() => {
        global.fetch = vi.fn().mockResolvedValue({
            json: async () => ({
                studentCount: 0,
                activeUserCount: 0,
                testsTakenPastWeek: [],
                flaggedItemCount: 0,
            }),
        } as Response);

        render(
            <MemoryRouter>
                <AdminHome />
            </MemoryRouter>
        );
    });

    it("renders the dashboard heading", () => {
        const heading = screen.getByRole("heading", { name: /researcher dashboard/i });
        expect(heading).toBeInTheDocument();
    });
});