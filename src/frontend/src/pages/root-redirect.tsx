/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { useEffect, useState,  } from 'react';
import { Navigate } from 'react-router-dom';
import { CheckAuth } from '../api/auth.ts';
import { UserIsAdmin } from "../api/user.ts";

/**
 * Redirects to /home or /admin/home if logged in, otherwise to specified redirect.
 * @param redirect the path to redirect to if not logged in
 * @returns a Navigate component that redirects to the appropriate page
 **/
export default function RootRedirect({ redirect }: { redirect: string }) {
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
    

    if (authorized === null) return null;

    return authorized ? <Navigate to={isAdmin ? "/admin/home" : "/home"} replace /> : <Navigate to={redirect} replace />;
}

/**
 * Redirects to /home or /admin/home if logged in, otherwise renders children.
 * @param children the components to render if not logged in
 * @returns a Navigate component that redirects to /home if logged in, otherwise renders children
 **/
export function HomeRedirect({ children }: { children: React.ReactNode }) {
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

    if (authorized === null) return null;


    if (authorized) {
        return <Navigate to={isAdmin ? "/admin/home" : "/home"} replace />;
    }

    return children;
}