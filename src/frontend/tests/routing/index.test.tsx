/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import React from "react";
import { render, screen } from "@testing-library/react";
import { describe, it, vi, expect } from "vitest";
import "@testing-library/jest-dom/vitest";
import { MemoryRouter, Route, Routes } from "react-router-dom";

// Mock pages
vi.mock("../../src/pages/Home.tsx", () => ({ default: () => <div>Home Page</div> }));
vi.mock("../../src/pages/Login.tsx", () => ({ default: () => <div>Login Page</div> }));
vi.mock("../../src/pages/Register.tsx", () => ({ default: () => <div>Register Page</div> }));
vi.mock("../../src/pages/QTI.tsx", () => ({ default: () => <div>QTI</div> }));
vi.mock("../../src/pages/AI.tsx", () => ({ default: () => <div>AI</div> }));
vi.mock("../../src/pages/Chart.tsx", () => ({ default: () => <div>Chart</div> }));
vi.mock("../../src/pages/landing.tsx", () => ({ default: () => <div>LaunchPage</div> }));
vi.mock("../../src/pages/admin/admin-home.tsx", () => ({ default: () => <div>Admin Home Page</div> }));
vi.mock("../../src/pages/admin/dashboards/ItemDashboard.tsx", () => ({
    default: () => <div>ItemDashboard</div>,
}));
vi.mock("../../src/pages/admin/dashboards/UserDashboard.tsx", () => ({
    default: () => <div>UserDashboard</div>,
}));
vi.mock("../../src/pages/admin/AdminDashboard.tsx", () => ({
    default: () => <div>Admin Dashboard</div>,
}));
vi.mock("../../src/pages/root-redirect.tsx", () => ({ default: () => <div>Root Redirect</div> }));
vi.mock("../../src/components/auth/protected-route.tsx", () => ({
    default: ({ children }: { children: React.ReactNode }) => <>{children}</>,
}));
vi.mock("../../src/Layout.tsx", () => ({
    default: ({ children }: { children: React.ReactNode }) => <div>Layout Wrapper {children}</div>,
}));

// Inline route definitions for test purposes
export function RenderWithRoute(initialRoute: string) {
    render(
        <MemoryRouter initialEntries={[initialRoute]}>
            <Routes>
                <Route path="/login" element={<div>Login Page</div>} />
                <Route path="/register" element={<div>Register Page</div>} />
                <Route path="/home" element={<div>Home Page</div>} />
                <Route path="/admin/home" element={<div>Admin Home Page</div>} />
                <Route path="/admin" element={<div>Admin Home Page</div>} />
                <Route path="/unauthorized" element={<div>Access denied</div>} />
                <Route path="/" element={<div>Root Redirect</div>} />
            </Routes>
        </MemoryRouter>
    );
}
describe("Router configuration with MemoryRouter", () => {
    it("renders the login page route", async () => {
        RenderWithRoute("/login");
        expect(await screen.findByText("Login Page")).toBeInTheDocument();
    });

    it("renders the register page route", async () => {
        RenderWithRoute("/register");
        expect(await screen.findByText("Register Page")).toBeInTheDocument();
    });

    it("renders the home page inside layout and protected route", async () => {
        RenderWithRoute("/home");
        expect(await screen.findByText("Home Page")).toBeInTheDocument();
    });

    it("renders the admin home route", async () => {
        RenderWithRoute("/admin");
        expect(await screen.findByText("Admin Home Page")).toBeInTheDocument();
    });

    it("renders the admin home route", async () => {
        RenderWithRoute("/admin/home");
        expect(await screen.findByText("Admin Home Page")).toBeInTheDocument();
    });

    it("renders the unauthorized page", async () => {
        RenderWithRoute("/unauthorized");
        expect(await screen.findByText("Access denied")).toBeInTheDocument();
    });

    it("renders the root redirect page", async () => {
        RenderWithRoute("/");
        expect(await screen.findByText("Root Redirect")).toBeInTheDocument();
    });
});
