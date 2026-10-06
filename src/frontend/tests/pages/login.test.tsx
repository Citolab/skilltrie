/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen, fireEvent, waitFor, cleanup, queryByAttribute } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import "@testing-library/jest-dom/vitest";
import LoginPage from "../../src/pages/login";
import { FetchJson } from "../../src/utils/fetch-json";
import { Login } from "../../src/api/auth";

// Mock navigation
const mockNavigate = vi.fn();
vi.mock("react-router-dom", async () => {
    const actual = await vi.importActual("react-router-dom");
    return {
        ...actual,
        useNavigate: () => mockNavigate,
    };
});

vi.mock("../../src/utils/fetch-json", () => ({
    FetchJson: vi.fn(),
}));

// Tests for components of the login-form by simulating a page with a login form for every test
describe("Login Page", () => {
    // Clean up mocks, reset faked fetch results and clean up (clear) the rendered components
    beforeEach(() => {
        vi.clearAllMocks();
        cleanup();
        render(
            <MemoryRouter>
                <LoginPage />
            </MemoryRouter>
        );
    });

    it("shows error when fields are empty on submit", async () => {
        const submitButton = document.getElementById("submit");

        if (submitButton)
            fireEvent.click(submitButton);

        await waitFor(() => {
            expect(screen.getByText("Please fill in all fields.")).toBeInTheDocument();
        });
    });

    it("updates state on input change", () => {
        const emailInput = document.getElementById("email");
        const passwordInput = document.getElementById("password");

        if (emailInput)
            fireEvent.change(emailInput, { target: { value: "test@example.com", name: "email" } });

        if (passwordInput)
            fireEvent.change(passwordInput, { target: { value: "password123", name: "password" } });

        expect((emailInput as HTMLInputElement).value).toBe("test@example.com");
        expect((passwordInput as HTMLInputElement).value).toBe("password123");
    });

    it("hides the password input text", () => {
        const passwordInput = document.getElementById("password");

        expect(passwordInput).toHaveAttribute("type", "password");
    });

    it("allows the rememberme button to be toggled (off to on to off)", () => {
        const rememberMe = screen.getByRole("checkbox", { name: /remember me/i });

        expect(rememberMe).not.toBeChecked();

        fireEvent.click(rememberMe);
        expect(rememberMe).toBeChecked();

        fireEvent.click(rememberMe);
        expect(rememberMe).not.toBeChecked();
    });

    it("shows error message if login fails", async () => {
        const mockFetchJson = vi.mocked(FetchJson);
        mockFetchJson.mockRejectedValueOnce(new Error("Request failed with status 401"));

        const emailInput = document.getElementById("email");
        const passwordInput = document.getElementById("password");
        const submitButton = document.getElementById("submit");

        if (emailInput)
            fireEvent.change(emailInput, { target: { name: "email", value: "user@test.com" } });

        if (passwordInput)
            fireEvent.change(passwordInput, { target: { name: "password", value: "wrong" } });

        if (submitButton)
            fireEvent.click(submitButton);

        //await expect(Login({ email, password }, false)).rejects.toThrow();

        await waitFor(() => {
            expect(screen.getByText("Error logging in. Please try again.")).toBeInTheDocument();
        });
    });
});

describe("Login() function", () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    it("calls FetchJson with session cookies when rememberme = false", async () => {
        const payload = { email: "user@test.com", password: "Abc12!" };
        const response = { ok: true };

        vi.mocked(FetchJson).mockResolvedValueOnce(response);

        const result = await Login(payload, false);

        expect(FetchJson).toHaveBeenCalledWith("/api/auth/login", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(payload),
            credentials: "include",
        });

        expect(result).toBe(response);
    });

    it("calls FetchJson with persistent cookies when rememberme = true", async () => {
        const payload = { email: "user@test.com", password: "Abc12!" };
        const response = { ok: true };

        vi.mocked(FetchJson).mockResolvedValueOnce(response);

        const result = await Login(payload, true);

        expect(FetchJson).toHaveBeenCalledWith("/api/auth/login?useCookies=true", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(payload),
            credentials: "include",
        });

        expect(result).toBe(response);
    });

    it("throws an error if FetchJson rejects", async () => {
        const payload = { email: "user@test.com", password: "wrong" };
        const error = new Error("401 Unauthorized");

        vi.mocked(FetchJson).mockRejectedValueOnce(error);

        await expect(Login(payload, false)).rejects.toThrow("401 Unauthorized");
    });
});
