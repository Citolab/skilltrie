/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import "@testing-library/jest-dom/vitest";
import { render, screen, fireEvent } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach } from "vitest";
import BuySellButton from "../../../src/components/customisation/buy-sell-button";
import { type Cosmetic } from "../../../src/types/cosmetic";

describe("BuySellButton", () => {
    beforeEach(() => {
        vi.restoreAllMocks();
    });
    const fakeCosmetic: Partial<Cosmetic> = {
        id: 10,
        price: 50,
        iconFile: "/assets/hat.png",
    };
    it("renders the cosmetic icon and price", () => {
        render(
            <BuySellButton
                cosmetic={fakeCosmetic as Cosmetic}
                bought={false}
                onClickBuy={() => {}}
                onClickSell={() => {}}
            />
        );

        expect(screen.getByRole("img")).toHaveAttribute("src", "/assets/hat.png");
        expect(screen.getByText("50")).toBeInTheDocument();
    });

    it("displays 'Owned' badge when bought", () => {
        render(
            <BuySellButton
                cosmetic={fakeCosmetic as Cosmetic}
                bought={true}
                onClickBuy={() => {}}
                onClickSell={() => {}}
            />
        );

        expect(screen.getByText("Owned")).toBeInTheDocument();
    });

    it("does not display 'Owned' badge when not bought", () => {
        render(
            <BuySellButton
                cosmetic={fakeCosmetic as Cosmetic}
                bought={false}
                onClickBuy={() => {}}
                onClickSell={() => {}}
            />
        );

        expect(screen.queryByText("Owned")).not.toBeInTheDocument();
    });

    it("enables Buy button only when not bought", () => {
        render(
            <BuySellButton
                cosmetic={fakeCosmetic as Cosmetic}
                bought={false}
                onClickBuy={() => {}}
                onClickSell={() => {}}
            />
        );

        expect(screen.getByText("Buy")).not.toBeDisabled();
        expect(screen.getByText("Sell")).toBeDisabled();
    });

    it("enables Sell button only when bought", () => {
        render(
            <BuySellButton
                cosmetic={fakeCosmetic as Cosmetic}
                bought={true}
                onClickBuy={() => {}}
                onClickSell={() => {}}
            />
        );

        expect(screen.getByText("Buy")).toBeDisabled();
        expect(screen.getByText("Sell")).not.toBeDisabled();
    });

    it("calls onClickBuy with correct id", () => {
        const onClickBuy = vi.fn();
        render(
            <BuySellButton
                cosmetic={fakeCosmetic as Cosmetic}
                bought={false}
                onClickBuy={onClickBuy}
                onClickSell={() => {}}
            />
        );

        fireEvent.click(screen.getByText("Buy"));
        expect(onClickBuy).toHaveBeenCalledWith(10);
    });

    it("calls onClickSell with correct id", () => {
        const onClickSell = vi.fn();
        render(
            <BuySellButton
                cosmetic={fakeCosmetic as Cosmetic}
                bought={true}
                onClickBuy={() => {}}
                onClickSell={onClickSell}
            />
        );

        fireEvent.click(screen.getByText("Sell"));
        expect(onClickSell).toHaveBeenCalledWith(10);
    });
});
