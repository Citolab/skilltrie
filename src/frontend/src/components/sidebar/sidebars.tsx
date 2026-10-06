/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { useState, type ReactElement } from "react";
import * as logo from "./sidebarlogos";
import {
    HomeIcon,
    UserGroupIcon,
    UsersIcon,
    ArchiveBoxIcon,
    AdjustmentsVerticalIcon,
    AcademicCapIcon,
    StarIcon,
} from "@heroicons/react/24/solid";
import { NavLink } from "react-router-dom";
import VersionBadge from "../general-components/version-badge.tsx";

interface NavLinkWrapper {
    label: string;
    href: string;
    icon: ReactElement;
}

function renderNavLinkWrapper(wrapper: NavLinkWrapper): ReactElement {
    return (
        <NavLink
            to={wrapper.href}
            className={({ isActive }) =>
                `flex items-center p-3 transition duration-75 rounded-lg group 
                    ${
                        isActive
                            ? "bg-nav-active-LD text-nav-text-LD--active"
                            : "hover:bg-nav-LD--hover hover:text-nav-text-LD--hover"
                    }`
            }
        >
            {wrapper.icon}
            <span className="flex-1 ms-3">{wrapper.label}</span>
        </NavLink>
    );
}

export function GenericSideBar(genericLinks: NavLinkWrapper[]) {
    const [isSidebarOpen, setIsSidebarOpen] = useState(true);

    function toggleSidebar() {
        setIsSidebarOpen(!isSidebarOpen);
    }

    return (
        <aside id="sidebar" className={"bg-nav-LD border-b sm:border-r border-nav-border-LD p-2"}>
            <div className="flex justify-center w-full">
                <button
                    className="block p-3 text-center font-bold m-1 sm:hidden 
                    text-nav-text-LD hover:bg-nav-LD--hover hover:text-nav-text-LD-hover rounded-full transition duration-75"
                    onClick={toggleSidebar}
                >
                    {isSidebarOpen ? logo.sidebarToggleDownLogo : logo.sidebarToggleUpLogo}
                </button>
            </div>
            <nav>
                <ul className={"space-y-2 " + (isSidebarOpen || "hidden md:block")}>
                    {genericLinks.map((navLink, index) => (
                        <li key={index} className="rounded text-nav-text-LD">
                            {renderNavLinkWrapper(navLink)}
                        </li>
                    ))}
                    <li className="pt-4 mt-4 border-t border-nav-border-LD dark:border-nav-border-LD text-center text-xs text-text-muted">
                        © Utrecht University (ICS).
                    </li>
                    <div className="flex justify-center">
                        <VersionBadge></VersionBadge>
                    </div>
                </ul>
            </nav>
        </aside>
    );
}

// adminLink
const adminHomeLink: NavLinkWrapper = {
    label: "Home",
    href: "/admin/home",
    icon: <HomeIcon className="w-6 h-6" />,
};
const adminGroupsLink: NavLinkWrapper = {
    label: "Groups",
    href: "/admin/groups",
    icon: <UserGroupIcon className="w-6 h-6" />,
};
const adminUserLink: NavLinkWrapper = {
    label: "Users",
    href: "/admin/users",
    icon: <UsersIcon className="w-6 h-6" />,
};
const adminQuestionLink: NavLinkWrapper = {
    label: "Questions",
    href: "/admin/questions",
    icon: <ArchiveBoxIcon className="w-6 h-6" />,
};
const adminSettingsLink: NavLinkWrapper = {
    label: "Settings",
    href: "/admin/settings",
    icon: <AdjustmentsVerticalIcon className="w-6 h-6" />,
};
const studentHomeLink: NavLinkWrapper = {
    label: "Student environment",
    href: "/home",
    icon: <AcademicCapIcon className="w-6 h-6" />,
};
const adminBadgeSettings: NavLinkWrapper = {
    label: "Badges",
    href: "/admin/badge-settings",
    icon: <StarIcon className="w-6 h-6" />,
};

export function AdminSideBar() {
    const links = [
        adminHomeLink,
        adminGroupsLink,
        adminUserLink,
        adminQuestionLink,
        adminSettingsLink,
        adminBadgeSettings,
        studentHomeLink,
    ];
    return GenericSideBar(links);
}
