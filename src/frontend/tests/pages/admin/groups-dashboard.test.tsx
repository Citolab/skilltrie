/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { describe, it, expect, vi } from "vitest";
import "@testing-library/jest-dom/vitest";

import GroupsDashboard from "../../../src/pages/admin/dashboards/group-dashboard.tsx";

// Mock all API calls used inside GroupsDashboard
vi.mock("../../../src/api/group", () => ({
    GetGroups: vi.fn().mockResolvedValue([]),
    GetGroup: vi.fn().mockResolvedValue(null),
    CreateGroup: vi.fn().mockResolvedValue({}),
    UpdateGroup: vi.fn().mockResolvedValue({}),
    DeleteGroup: vi.fn().mockResolvedValue({}),
}));

// Mock child components so the test stays lightweight
vi.mock("../../../src/components/admin-dashboard/admin-search-and-pagination.tsx", () => ({
    default: () => <div data-testid="mock-search-pagination" />,
}));

vi.mock("../../../src/components/admin-dashboard/admin-table/admin-table.tsx", () => ({
    default: () => <div data-testid="mock-admin-table" />,
}));

vi.mock("../../../src/components/admin-dashboard/admin-popup-window.tsx", () => ({
    default: () => <div data-testid="mock-popup-window" />,
}));

describe("GroupsDashboard page", () => {
    it("renders without crashing", async () => {
        render(<GroupsDashboard />, { wrapper: MemoryRouter });

        // The root container has data-testid="groups-dashboard"
        expect(await screen.findByTestId("groups-dashboard")).toBeInTheDocument();
    });
});