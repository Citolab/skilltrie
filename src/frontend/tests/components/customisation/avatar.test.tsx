/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { render, screen } from "@testing-library/react";
import Avatar from "../../../src/components/customisation/avatar";
import { describe, it, expect } from "vitest";
import "@testing-library/jest-dom/vitest";

describe("Avatar component", () => {
    function mockRive() {
        return <canvas data-testid="rive-canvas" />;
    }

    it("renders the rive component", () => {
        render(<Avatar RiveComponent={mockRive} previewText="Preview!" />);

        const riveCanvas = screen.getByTestId("rive-canvas");
        expect(riveCanvas).toBeInTheDocument();
    });

    it("renders the preview text", () => {
        render(<Avatar RiveComponent={mockRive} previewText="Hello World" />);

        expect(screen.getByText("Hello World")).toBeInTheDocument();
    });
});
