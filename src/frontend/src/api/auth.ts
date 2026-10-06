/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { FetchJson } from "../utils/fetch-json.ts";
const controllerUrl = "/api/auth/";

//These interfaces probably shouldn't be here
/**
 * response you get with authentication api's
 * includes userId for PostHog anonymization
 */
export interface AuthResponse {
    email: string;
    id: number;
}

/**
 * payload needed to login for users
 */
export interface LoginPayload {
    email: string;
    password: string;
}

/**
 * payload needed to register a user
 */
export interface RegisterPayload {
    firstName: string;
    infix: string | null;
    lastName: string;
    email: string;
    displayName: string;
    password: string;
}

/**
 * result you get for checkAuth
 */
export interface AuthResult {
    authorized: boolean;
    user: { email: string };
}

const ResetUserHelper = (() => {
    sessionStorage.removeItem("badges");
    sessionStorage.removeItem("badgesCount");
});

/**
 * Returns the users authentication.
 * For now you get a response if you are logged in
 * @return AuthResponse
 */
export async function PingAuth(): Promise<AuthResponse | null> {
    try {
        return await FetchJson<AuthResponse>(controllerUrl.concat("pingauth"), {
            method: "GET",
            credentials: "include",
        });
    } catch {
        return null;
    }
}

/**
 * login for user
 * @param payload : login
 * @return AuthResponse
 */
export async function Login(payload: LoginPayload, rememberme: boolean): Promise<AuthResponse> {
    const query = rememberme ? "?useCookies=true" : "";
    const response = FetchJson<AuthResponse>(controllerUrl.concat(`login${query}`), {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload),
        credentials: "include",
    });

    ResetUserHelper();

    return response;
}

/**
 * registers a new user
 * @param payload : RegisterPayload - this is what is needed to register the user
 * @return AuthResponse
 */
export async function RegisterApi(payload: RegisterPayload): Promise<AuthResponse> {
    return FetchJson<AuthResponse>(controllerUrl.concat("register"), {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload),
        credentials: "include",
    });
}

/**
 * Logs out the current user by sending a POST request.
 * Uses fetchJson for consistent error handling and typed responses.
 */
export async function Logout(): Promise<boolean> {
    try {
        await FetchJson<void>(controllerUrl.concat("logout"), {
            method: "POST",
            credentials: "include",
        });

        ResetUserHelper();

        return true;
    } catch (error) {
        console.error("Logout failed:", error);
        return false;
    }
}

/**
 * this is an api call to check if someone is logged in
 * this is the one that was in the authorize View
 * Roles are not implemented
 *
 * Useful for login gating, protected route checks, or conditional rendering based on auth status.
 * @param url - authentication api endpoint
 * @param maxRetries - max attempts to check
 * @param delay - delay between trying to connect
 * @return AuthResult - it returns if this user is authorized and some data
 */
export async function CheckAuth(
    url: string = controllerUrl.concat("pingauth"),
    maxRetries: number = 1,
    delay: number = 0
): Promise<AuthResult> {
    let retryCount = 0;

    function wait(ms: number): Promise<void> {
        return new Promise((resolve) => setTimeout(resolve, ms));
    }

    async function attempt(): Promise<AuthResult> {
        try {
            const response = await fetch(import.meta.env.BASE_URL.replace(/\/$/, '') + url, { method: "GET" });

            if (response.status === 200) {
                const data = (await response.json()) as AuthResponse;
                return { authorized: true, user: { email: data.email } };
            }

            return { authorized: false, user: { email: "" } };
        } catch (err) {
            retryCount++;
            if (retryCount > maxRetries) throw err;
            await wait(delay);
            return attempt();
        }
    }
    return attempt();
}
