/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { useNavigate } from "react-router-dom";
import { useEffect, useState } from "react";
import { CheckAuth } from "../api/auth.ts";
import { UserIsAdmin } from "../api/user.ts";
import GeneralButton from "../components/general-components/general-button";

export function NotFoundPage() {
    const navigate = useNavigate();

    const [authorized, setAuthorized] = useState<boolean | null>(null);

    const [isAdmin, setIsAdmin] = useState<boolean>(false);

    useEffect(() => {
        void UserIsAdmin().then(setIsAdmin);
    }, []);

    useEffect(() => {
        CheckAuth()
            .then(({ authorized }) => setAuthorized(authorized))
            .catch(() => setAuthorized(false));
    }, []);

    return (
        <div className="basicText flex justify-center flex-col gap-5">
            <h1 className="titleText text-center">404: Page not found</h1>
            <GeneralButton
                type="button"
                variant="primary"
                onClick={() =>
                    navigate(authorized ? (isAdmin ? "/admin/home" : "/home") : "/landing")
                }
            >
                Go To Home Page
            </GeneralButton>
        </div>
    );
}
