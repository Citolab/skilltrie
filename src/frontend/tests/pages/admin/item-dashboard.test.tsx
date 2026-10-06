/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { describe, it, expect, vi } from "vitest";
import ItemDashboard from "../../../src/pages/admin/dashboards/item-dashboard";
import "@testing-library/jest-dom/vitest";

// Mock the fetch-json utility so ItemDashboard doesn't hit the network
vi.mock("../../../src/utils/fetch-json", () => ({
    FetchJson: vi.fn().mockResolvedValue([]),
}));

describe("ItemDashboard page", () => {
    it("renders without crashing", async () => {
        render(<ItemDashboard />, { wrapper: MemoryRouter });

        // use findByTestId if the element appears after async rendering
        expect(await screen.findByTestId("item-dashboard")).toBeInTheDocument();
    });
});
