/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { vi, describe, test, beforeEach, afterEach, expect } from "vitest";
import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import "@testing-library/jest-dom/vitest";
import AiFeedbackPage from "../../src/components/qti-player/ai-feedback.tsx";

const initialFeedback = "Initial AI feedback";
const updatedFeedback = "Updated AI feedback";

// Mock API call
vi.mock("../../src/api/ai", () => ({
    FetchAiFeedback: vi.fn(),
}));

import { FetchAiFeedback } from "../../src/api/ai";

const mockedFetchAiFeedback = vi.mocked(FetchAiFeedback);

beforeEach(() => {
    mockedFetchAiFeedback.mockImplementation(async (testId, onChunk) => {
        onChunk("Initial AI feedback");
    });
});

afterEach(() => {
    vi.resetAllMocks();
});

describe("AiFeedbackPage component", () => {
    test("renders feedback text after fetch", async () => {
        render(<AiFeedbackPage testId={42} />);

        await waitFor(() => 
            expect(screen.getByText(/Initial AI feedback/i)).toBeInTheDocument(),
            { timeout: 1000 }
        );
    });




    test("shows error message when fetch fails", async () => {
        mockedFetchAiFeedback.mockRejectedValueOnce(new Error("API failed"));
        render(<AiFeedbackPage testId={42} />);

        const errorText = await screen.findByText("Failed to load feedback.");
        expect(errorText).toBeInTheDocument();
    });
});
