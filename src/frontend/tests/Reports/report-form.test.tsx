/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { vi, expect, describe, test, it } from "vitest";
import { render, screen, fireEvent } from "@testing-library/react";
import "@testing-library/jest-dom/vitest";
import ReportForm from "../../src/components/qti-player/popups/report-form.tsx";
import type { ItemErrorStruct, ReportPayload } from "../../src/types/report.ts";
import * as reportAPI from "../../src/api/report.ts";

const flavourOptions: ItemErrorStruct[] = [
    { value: "ATextEmpty", label: "Vanilla", category: "answer" },
    { value: "GraphicsUnavailable", label: "Chocolate", category: "other" },
    { value: "QTextIncorrect", label: "Mango", category: "question" },
];

export function RenderReport() {
    render(
        <ReportForm
            open={true}
            itemId={() => 4}
            onClose={() => {}}
            reportOptions={flavourOptions}
            reportDefault={flavourOptions[0]}
        />
    );
}

describe("Components present", () => {
    test("Form label present", () => {
        RenderReport();
        expect(screen.getByText("Question report form")).toBeInTheDocument();
    });

    test("Close button present", () => {
        RenderReport();
        expect(screen.getByRole("button", { name: /cancel/i })).toBeInTheDocument();
    });

    test("Issue description label present", () => {
        RenderReport();
        const labelText = "Please describe the issue with this question:";
        expect(screen.getByText(labelText)).toBeInTheDocument();
    });

    test("Submit button present", () => {
        RenderReport();
        expect(screen.getByRole("button", { name: /submit/i })).toBeInTheDocument();
    });

    test("Default option present", () => {
        RenderReport();
        expect(screen.getByText(flavourOptions[0].label)).toBeInTheDocument();
    });

    test("Other options absent", () => {
        RenderReport();
        expect(screen.queryByText(flavourOptions[1].label)).not.toBeInTheDocument();
    });
});

describe("OnSubmit behaviour", () => {
    it("Called with correct itemId and default selection", () => {
        const createReportSpy = vi.spyOn(reportAPI, "CreateReport");

        RenderReport();
        const button = screen.getByRole("button", { name: /submit/i });
        fireEvent.click(button);

        expect(createReportSpy).toHaveBeenCalledWith({
            itemId: 4,
            itemError: flavourOptions[0].value,
        } as ReportPayload);
    });

    it("Called with correct itemId and non-default selection", () => {
        const createReportSpy = vi.spyOn(reportAPI, "CreateReport");

        RenderReport();
        const combo = screen.getByRole("combobox");
        fireEvent.keyDown(combo, { key: "ArrowDown" });

        const option = screen.getByText(flavourOptions[1].label);
        fireEvent.click(option);

        const button = screen.getByRole("button", { name: /submit/i });
        fireEvent.click(button);

        expect(createReportSpy).toHaveBeenCalledWith({
            itemId: 4,
            itemError: flavourOptions[1].value,
        } as ReportPayload);
    });
});
