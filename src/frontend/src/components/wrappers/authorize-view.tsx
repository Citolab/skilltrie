/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import React, { useState, useEffect, createContext } from "react";

const UserContext = createContext({});

interface User {
    email: string;
}

// When wrapping this around a component, said component will only be visible when logged in.
// Use this when you want to differentiate component visibility between being logged in and logged out

// !! DO NOT USE YET for view handling based on roles, that is not implemented in this wrapper !!
function AuthorizeView(props: { children: React.ReactNode }) {
    const [authorized, setAuthorized] = useState<boolean>(false);
    const [loading, setLoading] = useState<boolean>(true);
    const emptyuser: User = { email: "" };

    const [user, setUser] = useState(emptyuser);

    useEffect(() => {
        let retryCount = 0;
        const maxRetries = 10;
        const delay = 1000;

        function wait(delay: number) {
            return new Promise((resolve) => setTimeout(resolve, delay));
        }

        // define a fetch function that retries until status 200 or 401
        async function fetchWithRetry(url: string, options: any) {
            try {
                const response = await fetch(url, options);

                if (response.status === 200) {
                    console.log("Authorized");
                    const j: any = await response.json();
                    setUser({ email: j.email });
                    setAuthorized(true);
                    return response; // return the response
                } else if (response.status === 401) {
                    console.log("Unauthorized");
                    return response; // return the response
                } else {
                    // throw an error to trigger the catch block
                    throw new Error("" + response.status);
                }
            } catch (error) {
                retryCount++;
                if (retryCount > maxRetries) {
                    throw error;
                } else {
                    await wait(delay);
                    return fetchWithRetry(url, options);
                }
            }
        }

        fetchWithRetry("/api/auth/pingauth", {
            method: "GET",
        })
            .catch((error) => {
                console.log(error.message);
            })
            .finally(() => {
                setLoading(false);
            });
    }, []);

    if (loading) {
        return <>{/* <p>Loading...</p> */}</>;
    } else {
        if (authorized && !loading) {
            return (
                <>
                    <UserContext.Provider value={user}>{props.children}</UserContext.Provider>
                </>
            );
        } else {
            return (
                <>
                    {/* <Navigate to="/login" />  | Change to this when login authorization/isolation is fully implemented, otherwise this reloads pages you dont want to have reloaded */}
                </>
            );
        }
    }
}

export function AuthorizedUser(props: { value: string }) {
    // Consume the displayname from the UserContext
    const user: any = React.useContext(UserContext);

    // Display the displayname in a h1 tag
    if (props.value === "email") return <>{user.email}</>;
    else if (props.value === "displayname") return <>{user.displayname}</>;
    else return <></>;
}

export default AuthorizeView;
