/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { NavLink, Outlet } from "react-router-dom";
import { AdminSideBar } from "../components/sidebar/sidebars.tsx";
import { useEffect, useState } from "react";
import { GetActiveUser, UserIsAdmin } from "../api/user.ts";
import { Link } from "react-router-dom";
import * as logo from "../components/sidebar/sidebarlogos";
import LogoutLink from "../components/wrappers/logout-link";
import VersionBadge from "../components/general-components/version-badge.tsx";
import { UserIcon } from "@heroicons/react/24/solid";

function StudentNavBar({ username }: { username: string }) {
    return (
        <nav className="z-50 flex-none w-full bg-white border-b border-gray-200 dark:bg-gray-800 dark:border-gray-700">
            <div className="px-3 py-3 lg:px-5 lg:pl-3">
                <div className="flex items-center justify-between">
                    <div className="flex items-center justify-start rtl:justify-end">
                        <Link to={"/home"} className="flex items-center">
                            <img
                                src="/skilltrie_logo.svg"
                                alt="skilltrie logo"
                                className="w-14 h-14 rounded-full mr-2"
                            />
                            <div className="text-2xl font-bold text-primary">
                                SKILLTRIE <VersionBadge></VersionBadge>
                            </div>
                        </Link>
                    </div>
                    <div className="flex gap-3">
                        <div
                            className={`flex items-center justify-end gap-2 p-2 hover:text-nav-text-LD-hover hover:bg-nav-LD--hover
                        transition duration-75 rounded-lg group`}
                        >
                            <NavLink
                                to={`/profile/${username}`}
                                className={({ isActive }) =>
                                    `flex items-center gap-2 ${isActive ? "text-primary" : "text-nav-text-LD"}`
                                }
                            >
                                <UserIcon className="size-6" />
                                <p className="inline">Profile</p>
                            </NavLink>
                        </div>
                        <div
                            className="flex items-center justify-end gap-2 p-2 text-nav-text-LD hover:text-nav-text-LD-hover hover:bg-nav-LD--hover
                        transition duration-75 rounded-lg group"
                        >
                            <LogoutLink>
                                {logo.logoutLinkLogo}
                                Logout
                            </LogoutLink>
                        </div>
                    </div>
                </div>
            </div>
        </nav>
    );
}

function AdminNavBar() {
    return (
        <nav className="flex-none w-full bg-nav-LD border-b border-nav-border-LD ">
            <div className="px-3 py-3 lg:px-5 lg:pl-3">
                <div className="flex items-center justify-between">
                    <div className="flex items-center justify-start rtl:justify-end">
                        <Link to={"/admin/home"} className="flex items-center">
                            <img
                                src="/skilltrie_logo.svg"
                                alt="skilltrie logo"
                                className="w-14 h-14 rounded-full mr-2"
                            />
                            <div className="text-2xl font-bold text-primary">SKILLTRIE</div>
                        </Link>
                    </div>
                    <div
                        className="flex items-center justify-end gap-2 p-2 text-nav-text-LD hover:text-nav-text-LD-hover hover:bg-nav-LD--hover
                     transition duration-75 rounded-lg group"
                    >
                        <LogoutLink>
                            {logo.logoutLinkLogo}
                            Logout
                        </LogoutLink>
                    </div>
                </div>
            </div>
        </nav>
    );
}

export default function Layout() {
    const [isAdmin, setIsAdmin] = useState<boolean>(false);
    const [username, setUsername] = useState<string>("");

    useEffect(() => {
        void UserIsAdmin().then(setIsAdmin);
    }, []);

    useEffect(() => {
        void GetActiveUser().then((user) => setUsername(user.displayName));
    }, []);

    return (
        <div className="flex flex-col h-dvh">
            {isAdmin ? <AdminNavBar /> : <StudentNavBar username={username}></StudentNavBar>}
            <div className="flex flex-1 min-h-0 flex-col sm:flex-row">
                {isAdmin && <AdminSideBar />}
                <main className="flex-1 overflow-y-auto bg-gray-50 dark:bg-gray-900">
                    <Outlet />
                </main>
            </div>
        </div>
    );
}
