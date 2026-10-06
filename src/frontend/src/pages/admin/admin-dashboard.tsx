/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { Outlet } from "react-router-dom";
import "./admin-dashboard.css";
import { useEffect } from "react";

/** Wrapper for all admin dashboards */
export default function AdminDashboard() {
    // add a less intrusive scroll bar for chromium based browser
    useEffect(() => {
        document.body.classList.add("adminScrollbar");
        return () => {
            document.body.classList.remove("adminScrollbar");
        };
    }, []);

    // ensures equal background color for all admin pages
    return (
        <div id="admin-wrapper" className="bg-admin-surface w-full min-h-full">
            <Outlet />
        </div>
    );
}
