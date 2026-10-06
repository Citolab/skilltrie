/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import "@testing-library/jest-dom/vitest";
import { render, screen } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach } from "vitest";
import { MemoryRouter } from "react-router-dom";
import { CustomisationHeader } from "../../../src/components/customisation/customisation-header";
import { type UserCurrency, type Currency } from "../../../src/types/currency";
import { type CurrencyProfileProps } from "../../../src/components/customisation/currency-profile";

describe("CustomisationHeader", () => {
    beforeEach(() => {
        vi.restoreAllMocks();
    });
    vi.mock("./currency-profile.tsx", () => ({
        default: (props: CurrencyProfileProps) => (
            <div data-testid="currency-profile-mock">{JSON.stringify(props)}</div>
        ),
    }));

    const props: CurrencyProfileProps = {
        currencies: [{ id: 1, name: "Gold", sprite: "gold.png" }] as Currency[],
        userCurrencies: [{ currencyId: 1, amount: 100 }] as UserCurrency[],
    };

    it("renders navigation links", () => {
        render(
            <MemoryRouter>
                <CustomisationHeader {...props} />
            </MemoryRouter>
        );

        expect(screen.getByText("Inventory")).toBeInTheDocument();
        expect(screen.getByText("Shop")).toBeInTheDocument();
    });

    it("renders the CurrencyProfile component with provided props", () => {
        render(
            <MemoryRouter>
                <CustomisationHeader {...props} />
            </MemoryRouter>
        );
        expect(screen.getByText("Balance:")).toBeInTheDocument();
        expect(screen.getByText(100)).toBeInTheDocument();
    });

    it("applies active styling based on route", () => {
        render(
            <MemoryRouter initialEntries={["/inventory"]}>
                <CustomisationHeader {...props} />
            </MemoryRouter>
        );

        const inventoryLink = screen.getByText("Inventory");
        const shopLink = screen.getByText("Shop");

        expect(inventoryLink.className).toContain("border-b");

        expect(shopLink.className).toContain("text-slate-300");
    });
});
