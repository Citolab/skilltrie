/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { render, screen } from "@testing-library/react";
import "@testing-library/jest-dom/vitest";
import { describe, it, expect, beforeEach, vi } from "vitest";
import CurrencyProfile from "../../../src/components/customisation/currency-profile";

import { type Currency, type UserCurrency } from "../../../src/types/currency";

describe("CurrencyProfile", () => {
    beforeEach(() => {
        vi.restoreAllMocks();
    });
    const currencies = [
        { id: 1, name: "Gold", sprite: "gold.png" },
        { id: 2, name: "Gems", sprite: "gems.png" },
    ] as Currency[]; // minimal fields required

    const userCurrencies = [
        { currencyId: 1, amount: 500 },
        { currencyId: 2, amount: 20 },
    ] as UserCurrency[];

    it("renders the balance label", () => {
        render(<CurrencyProfile currencies={currencies} userCurrencies={userCurrencies} />);
        expect(screen.getByText("Balance:")).toBeInTheDocument();
    });

    it("renders a balance entry for each user currency", () => {
        render(<CurrencyProfile currencies={currencies} userCurrencies={userCurrencies} />);

        // amounts visible
        expect(screen.getByText("500")).toBeInTheDocument();
        expect(screen.getByText("20")).toBeInTheDocument();
    });

    it("renders the correct sprite for each currency", () => {
        render(<CurrencyProfile currencies={currencies} userCurrencies={userCurrencies} />);

        const images = screen.getAllByRole("img");

        expect(images[0]).toHaveAttribute("src", "gold.png");
        expect(images[1]).toHaveAttribute("src", "gems.png");
    });
});
