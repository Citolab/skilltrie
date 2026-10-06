/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import React, { useState, useEffect, createContext } from 'react';
import {CheckAuth} from "../../api/auth.ts";

const UserContext = createContext({});

interface User {
    email: string;
}

/**
* When wrapping this around a component, said component will only be visible when logged in.
* Use this when you want to differentiate component visibility between being logged in and logged out
* !! DO NOT USE YET for view handling based on roles, that is not implemented in this wrapper !!
*/
function AuthorizeView(props: { children: React.ReactNode }) {

    const [authorized, setAuthorized] = useState<boolean>(false);
    const [loading, setLoading] = useState<boolean>(true); // add a loading state
    const emptyuser: User = { email: "" };

    const [user, setUser] = useState(emptyuser);


    useEffect(() => {
        setLoading(true)
        CheckAuth()
            .then(({ authorized,user }) => {
                setAuthorized(authorized);
                setLoading(false)
                setUser(user);
            })
            .catch(() => setAuthorized(false));
    }, []);

    if (loading) {
        return (
            <>
                { /* <p>Loading...</p> */ }
            </>
        );
    }
    else {
        if (authorized && !loading) {
            return (
                <>
                    <UserContext.Provider value={user}>{props.children}</UserContext.Provider>
                </>
            );
        } else {
            return (

                <>
                     { /* <Navigate to="/login" />  | Change to this when login authorization/isolation is fully implemented, otherwise this reloads pages you dont want to have reloaded */ }
                </>

            )
        }
    }

}
export default AuthorizeView;
