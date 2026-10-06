/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { render, screen, waitFor } from "@testing-library/react";
import { MemoryRouter, Routes, Route } from "react-router-dom";
import { describe, it, vi, expect, beforeEach } from "vitest";
import ProtectedRoute from "../../../src/components/auth/protected-route.tsx";
import type { AuthResult } from "../../../src/api/auth.ts";
import "@testing-library/jest-dom/vitest";

vi.mock("../../../src/api/auth.ts", () => ({
    CheckAuth: vi.fn(),
}));

import { CheckAuth } from "../../../src/api/auth.ts";

const mockedCheckAuth = vi.mocked(CheckAuth);

export function MockPage() {
    return <div>Protected Content</div>;
}
export function RedirectPage() {
    return <div>Redirected</div>;
}
export function FallbackPage() {
    return <div>Loading...</div>;
}

describe("ProtectedRoute", () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    function renderWithRoute() {
        render(
            <MemoryRouter initialEntries={["/protected"]}>
                <Routes>
                    <Route path="/redirect" element={<RedirectPage />} />
                    <Route
                        element={
                            <ProtectedRoute redirectTo="/redirect" fallback={<FallbackPage />} />
                        }
                    >
                        <Route path="/protected" element={<MockPage />} />
                    </Route>
                </Routes>
            </MemoryRouter>
        );
    }
    it("shows fallback while loading", () => {
        mockedCheckAuth.mockReturnValue(new Promise(() => {}));
        renderWithRoute();
        expect(screen.getByText("Loading...")).toBeInTheDocument();
    });

    it("renders protected content when authorized", async () => {
        mockedCheckAuth.mockResolvedValue({ authorized: true } as AuthResult);
        renderWithRoute();
        expect(await screen.findByText("Protected Content")).toBeInTheDocument();
    });

    it("redirects when not authorized", async () => {
        mockedCheckAuth.mockResolvedValue({ authorized: false } as AuthResult);
        renderWithRoute();
        await waitFor(() => {
            expect(screen.getByText("Redirected")).toBeInTheDocument();
        });
    });

    it("redirects on auth check failure", async () => {
        mockedCheckAuth.mockRejectedValue(new Error("Network error"));
        renderWithRoute();
        await waitFor(() => {
            expect(screen.getByText("Redirected")).toBeInTheDocument();
        });
    });
});
