/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { expect, describe, it, vi, beforeEach } from "vitest";
import React from "react";
import { render, screen, waitFor } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import "@testing-library/jest-dom/vitest";
import Layout from "../../src/router/layout";

// Mock nested components
vi.mock("../../src/components/auth/authorize-view.tsx", () => ({
    default: ({ children }: { children: React.ReactNode }) => <>{children}</>,
}));

vi.mock("../../src/components/wrappers/LogoutLink.tsx", () => ({
    default: ({ children }: { children: React.ReactNode }) => <>{children}</>,
}));

// Mock API call
vi.mock("../../src/api/user", () => ({
    UserIsAdmin: vi.fn(),
    GetActiveUser: vi.fn(),
}));

import { GetActiveUser, UserIsAdmin } from "../../src/api/user";
import { defaultUser } from "../../src/types/user";

const mockedUserIsAdmin = vi.mocked(UserIsAdmin);
const mockedGetActiveUser = vi.mocked(GetActiveUser);

describe("Layout", () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    describe("when user is NOT admin", () => {
        beforeEach(async () => {
            mockedUserIsAdmin.mockResolvedValue(false);
            mockedGetActiveUser.mockResolvedValue(defaultUser);

            render(
                <MemoryRouter>
                    <Layout />
                </MemoryRouter>
            );

            await waitFor(() => {
                expect(UserIsAdmin).toHaveBeenCalled();
            });
        });

        it("renders the brand name", () => {
            expect(screen.getByText("SKILLTRIE")).toBeInTheDocument();
        });

        it("renders the header with base navigation links", () => {
            expect(screen.getAllByRole("navigation")).toHaveLength(1);
            expect(screen.getByText("Logout")).toBeInTheDocument();
        });

        it("does not render admin navigation links,", () => {
            expect(screen.queryByText("Groups")).not.toBeInTheDocument();
            expect(screen.queryByText("Users")).not.toBeInTheDocument();
            expect(screen.queryByText("Questions")).not.toBeInTheDocument();
            expect(screen.queryByText("Settings")).not.toBeInTheDocument();
            expect(screen.queryByText("Student environment")).not.toBeInTheDocument();
        });
    });

    describe("when user IS admin", () => {
        beforeEach(async () => {
            mockedUserIsAdmin.mockResolvedValue(true);
            mockedGetActiveUser.mockResolvedValue(defaultUser);

            render(
                <MemoryRouter>
                    <Layout />
                </MemoryRouter>
            );

            await waitFor(() => {
                expect(UserIsAdmin).toHaveBeenCalled();
            });
        });

        it("renders the brand name", () => {
            expect(screen.getByText("SKILLTRIE")).toBeInTheDocument();
        });

        // it("renders the header with base navigation links", () => {
        //     expect(screen.getAllByRole("navigation")).toHaveLength(1);
        //     expect(screen.getByText("Logout")).toBeInTheDocument();
        // });

        it("renders admin navigation links", () => {
            expect(screen.getByText("Home")).toBeInTheDocument();
            expect(screen.getByText("Groups")).toBeInTheDocument();
            expect(screen.getByText("Users")).toBeInTheDocument();
            expect(screen.getByText("Questions")).toBeInTheDocument();
            expect(screen.getByText("Settings")).toBeInTheDocument();
            expect(screen.getByText("Student environment")).toBeInTheDocument();
        });
    });
});
