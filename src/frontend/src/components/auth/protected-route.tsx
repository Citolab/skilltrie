/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { useEffect, useState } from 'react';
import { Navigate, Outlet } from 'react-router-dom';
import {CheckAuth} from "../../api/auth.ts";
import { UserIsAdmin } from '../../api/user.ts';

interface ProtectedRouteProps {
    redirectTo?: string;
    fallback?: React.ReactNode;
    adminAccessRequired?: boolean;
}

/**
 * wrapper to make certain routes accessible for certain roles
 * works for both login-based and role-based authorization
 *
 * @param redirectTo - redirect if there is no access to a page
 * @param fallback - fallback if api doesn't react fast
 * @param adminAccessRequired - whether admin role is required to view and access the page
 * @constructor
 */
export default function ProtectedRoute({
                                           redirectTo = '/',
                                           fallback = null,
                                           adminAccessRequired = false,
                                       }: ProtectedRouteProps) {

    const [authorized, setAuthorized] = useState<boolean | null>(null);

    useEffect(() => {
        CheckAuth()
            .then(({ authorized,  }) => {
                if (!adminAccessRequired) {
                    setAuthorized(authorized);
                    return;
                }
                UserIsAdmin()
                    .then((isAdmin) => {
                        setAuthorized(authorized && isAdmin);
                    })
                    .catch(() => setAuthorized(false));
            })
            .catch(() => setAuthorized(false));
    }, []);

    if (authorized === null) return <>{fallback}</>;

    if (!authorized) return <Navigate to={redirectTo} replace />;

    return <Outlet />;
}
