/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, waitFor, screen } from "@testing-library/react";
import { MemoryRouter, Routes, Route, useLocation } from "react-router-dom";
import RootRedirect from "../../src/pages/root-redirect";
import * as authApi from "../../src/api/auth";
import * as userApi from "../../src/api/user";
import "@testing-library/jest-dom/vitest";

// Helper component to expose current location
export function LocationDisplay() {
    const location = useLocation();
    return <div data-testid="location">{location.pathname}</div>;
}

describe("RootRedirect", () => {
    const mockCheckAuth = vi.fn();
    const mockUserIsAdmin = vi.fn();

    beforeEach(() => {
        vi.clearAllMocks();
        vi.spyOn(authApi, "CheckAuth").mockImplementation(mockCheckAuth);
        vi.spyOn(userApi, "UserIsAdmin").mockImplementation(mockUserIsAdmin);
    });

    it("renders nothing while authorization is loading", () => {
        mockUserIsAdmin.mockResolvedValueOnce(false);
        mockCheckAuth.mockReturnValue(new Promise(() => {})); // never resolves
        const { container } = render(
            <MemoryRouter initialEntries={["/"]}>
                <Routes>
                    <Route path="/" element={<RootRedirect />} />
                </Routes>
            </MemoryRouter>
        );
        expect(container.firstChild).toBeNull();
    });

    it("redirects to /home when authorized and student", async () => {
        mockCheckAuth.mockResolvedValueOnce({ authorized: true });
        mockUserIsAdmin.mockResolvedValueOnce(false);

        render(
            <MemoryRouter initialEntries={["/"]}>
                <Routes>
                    <Route
                        path="/"
                        element={
                            <>
                                <RootRedirect redirect="/landing" />
                                <LocationDisplay />
                            </>
                        }
                    />
                    <Route path="/home" element={<LocationDisplay />} />
                </Routes>
            </MemoryRouter>
        );

        await waitFor(() => {
            expect(screen.getByTestId("location")).toHaveTextContent("/home");
        });
    });


    it("redirects to /admin/home when authorized and admin", async () => {
        mockCheckAuth.mockResolvedValueOnce({ authorized: true });
        mockUserIsAdmin.mockResolvedValueOnce(true);

        render(
            <MemoryRouter initialEntries={["/"]}>
                <Routes>
                    <Route
                        path="/"
                        element={
                            <>
                                <RootRedirect redirect="/landing" />
                                <LocationDisplay />
                            </>
                        }
                    />
                    <Route path="/admin/home" element={<LocationDisplay />} />
                </Routes>
            </MemoryRouter>
        );

        await waitFor(() => {
            expect(screen.getByTestId("location")).toHaveTextContent("/admin/home");
        });
    });


    it("redirects to /landing when not authorized", async () => {
        mockCheckAuth.mockResolvedValueOnce({ authorized: false });
        mockUserIsAdmin.mockResolvedValueOnce(false);

        render(
            <MemoryRouter initialEntries={["/"]}>
                <Routes>
                    <Route
                        path="/"
                        element={
                            <>
                                <RootRedirect redirect="/landing" />
                                <LocationDisplay />
                            </>
                        }
                    />
                    <Route path="/landing" element={<LocationDisplay />} />
                </Routes>
            </MemoryRouter>
        );

        await waitFor(() => {
            expect(screen.getByTestId("location")).toHaveTextContent("/landing");
        });
    });

    it("redirects to /landing on API error", async () => {
        mockCheckAuth.mockRejectedValueOnce(new Error("Network error"));
        mockUserIsAdmin.mockResolvedValueOnce(false);

        render(
            <MemoryRouter initialEntries={["/"]}>
                <Routes>
                    <Route
                        path="/"
                        element={
                            <>
                                <RootRedirect redirect="/landing" />
                                <LocationDisplay />
                            </>
                        }
                    />
                    <Route path="/landing" element={<LocationDisplay />} />
                </Routes>
            </MemoryRouter>
        );

        await waitFor(() => {
            expect(screen.getByTestId("location")).toHaveTextContent("/landing");
        });
    });
});
