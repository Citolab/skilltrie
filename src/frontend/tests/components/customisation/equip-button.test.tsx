/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import "@testing-library/jest-dom/vitest";

import { render, screen, fireEvent } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach } from "vitest";
import EquipButton from "../../../src/components/customisation/equip-button";
import { type Cosmetic } from "../../../src/types/cosmetic";

describe("EquipButton", () => {
    beforeEach(() => {
        vi.restoreAllMocks();
    });
    const fakeCosmetic: Partial<Cosmetic> = {
        id: 10,
        price: 50,
        iconFile: "/assets/hat.png",
    };
    it("renders the cosmetic image", () => {
        render(
            <EquipButton
                cosmetic={fakeCosmetic as Cosmetic}
                equipped={false}
                onClickEquip={() => {}}
            />
        );

        const img = screen.getByRole("img");
        expect(img).toHaveAttribute("src", "/assets/hat.png");
    });

    it("adds green border when equipped", () => {
        render(
            <EquipButton
                cosmetic={fakeCosmetic as Cosmetic}
                equipped={true}
                onClickEquip={() => {}}
            />
        );

        const button = screen.getByRole("button");
        expect(button.className).toContain("border-green-600");
    });

    it("does not add green border when not equipped", () => {
        render(
            <EquipButton
                cosmetic={fakeCosmetic as Cosmetic}
                equipped={false}
                onClickEquip={() => {}}
            />
        );

        const button = screen.getByRole("button");
        expect(button.className).toContain("opacity-80");
        expect(button.className).not.toContain("border-green-600");
    });

    it("calls onClickEquip with toggled equipped state", () => {
        const mockFn = vi.fn();

        render(
            <EquipButton
                cosmetic={fakeCosmetic as Cosmetic}
                equipped={false}
                onClickEquip={mockFn}
            />
        );

        fireEvent.click(screen.getByRole("button"));
        expect(mockFn).toHaveBeenCalledWith(10, true);
    });

    it("toggles equipped from true to false", () => {
        const mockFn = vi.fn();

        render(
            <EquipButton
                cosmetic={fakeCosmetic as Cosmetic}
                equipped={true}
                onClickEquip={mockFn}
            />
        );

        fireEvent.click(screen.getByRole("button"));
        expect(mockFn).toHaveBeenCalledWith(10, false);
    });
});
