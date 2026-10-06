/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { createBrowserRouter, Navigate, Outlet } from "react-router-dom";
import Layout from "./layout";
import LoginPage from "../pages/login";
import Register from "../pages/register";
import TopicSelection from "../pages/qti/topic-selection.tsx";
import LaunchPage from "../pages/landing";
import ItemDashboard from "../pages/admin/dashboards/item-dashboard";
import UserDashboard from "../pages/admin/dashboards/user-dashboard";
import AdminDashboard from "../pages/admin/admin-dashboard";
import ProtectedRoute from "../components/auth/protected-route.tsx";
import RootRedirect, { HomeRedirect } from "../pages/root-redirect.tsx";
import Settings from "../pages/settings.tsx";
import GroupDashboard from "../pages/admin/dashboards/group-dashboard.tsx";
import GroupPage from "../pages/admin/dashboards/group-page.tsx";
import AdminHome from "../pages/admin/admin-home.tsx";
import { LevelContextProvider } from "../contexts/level-context.tsx";
import { QTI } from "../pages/qti";
import { Home } from "../pages/home";
import PrivacyStatement from "../pages/privacy-statement.tsx";
import { Profile } from "../pages/profile.tsx";
import { NotFoundPage } from "../pages/not-found.tsx";
import { BadgeSettings } from "../pages/admin/badge-settings.tsx";

export const router = createBrowserRouter(
    [
        {
            path: "/",
            element: <RootRedirect redirect="/landing" />,
        },
        {
            path: "/landing",
            element: (
                <HomeRedirect>
                    <LaunchPage />
                </HomeRedirect>
            ),
        },
        {
            path: "/login",
            element: (
                <HomeRedirect>
                    <LoginPage />
                </HomeRedirect>
            ),
        },
        {
            path: "/register",
            element: (
                <HomeRedirect>
                    <Register />
                </HomeRedirect>
            ),
        },
        {
            path: "/privacy-statement",
            element: <PrivacyStatement />,
        },
        {
            path: "/",
            element: (
                <LevelContextProvider>
                    <Outlet />
                </LevelContextProvider>
            ),
            children: [
                { path: "qti/", element: <QTI /> },
                {
                    element: (
                        <>
                            <Layout />
                            <ProtectedRoute />
                        </>
                    ),
                    children: [{ path: "qti/start", element: <TopicSelection /> }],
                },
                {
                    element: <Layout />,
                    children: [
                        {
                            element: <ProtectedRoute />,
                            children: [
                                { path: "/home", element: <Home /> },
                                { path: "/profile/:userName", element: <Profile /> },
                            ],
                        },
                    ],
                },
            ],
        },
        {
            element: <Layout />,
            children: [
                {
                    path: "/admin",
                    element: <ProtectedRoute adminAccessRequired={true} />, // toddo: make the roles work
                    children: [
                        {
                            path: "",
                            element: <AdminDashboard />,
                            children: [
                                {
                                    index: true,
                                    element: <Navigate to="home" replace />,
                                },
                                {
                                    path: "home",
                                    element: <AdminHome />,
                                },
                                {
                                    path: "users",
                                    element: <UserDashboard />,
                                },
                                {
                                    path: "questions",
                                    element: <ItemDashboard />,
                                },
                                {
                                    path: "settings",
                                    element: <Settings />,
                                },
                                {
                                    path: "groups",
                                    element: <GroupDashboard />,
                                },
                                {
                                    path: "groups/:id",
                                    element: <GroupPage />,
                                },
                                {
                                    path: "badge-settings/",
                                    element: <BadgeSettings />,
                                },
                            ],
                        },
                    ],
                },
                {
                    path: "*",
                    element: <NotFoundPage />,
                },
            ],
        },
        {
            path: "/unauthorized",
            element: <p>Access denied</p>,
        },
    ],
    {
        basename: import.meta.env.BASE_URL,
    }
);
