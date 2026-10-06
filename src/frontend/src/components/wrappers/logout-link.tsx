/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { useNavigate } from "react-router-dom";
import {Logout} from "../../api/auth.ts";
import React from "react";

/**
 * Wrap this around any element to make it a logout trigger.
 */
function LogoutLink(props: { children: React.ReactNode }) {
    const navigate = useNavigate();

    async function handleSubmit(e: React.FormEvent<HTMLAnchorElement>) {
        e.preventDefault();

        try {
            await Logout();
            await navigate("/");
            window.location.reload();
        } catch (error) {
            console.error("Logout failed:", error);
        }
    }

    return (
        <a
            href="#"
            onClick={(e) => {void handleSubmit(e);}}
            className="flex items-center gap-2"
        >
            {props.children}
        </a>
    );
}

export default LogoutLink;
