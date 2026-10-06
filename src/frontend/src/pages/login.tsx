/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

/**
 *
 * Login form for handling user logins to access the site.
 * @remarks .NET Identity handles hashing and validating the password with what's stored in the backend for us. DO NOT manually add a hasher.
 *
 */

import { useState } from "react";
import { useNavigate, Link } from "react-router-dom";
import { Login, type AuthResponse, type LoginPayload as Payload } from "../api/auth";
import Form from "../components/general-components/form";
import TextField from "../components/general-components/textfield-mui";
import Checkbox from "../components/login/form/checkbox";
import { usePostHog } from 'posthog-js/react';
import { Title } from "../components/general-components/title.tsx";

export default function LoginPage() {
    const [email, setEmail] = useState<string>("");
    const [password, setPassword] = useState<string>("");
    const [checked, setChecked] = useState<boolean>(false);
    const [error, setError] = useState<string>("");
    const navigate = useNavigate();
    const posthog = usePostHog();

    async function submitHandler(success: boolean) {
        if (success)
        {
            const payload: Payload = {
                email: email.trim(),
                password: password
            };

            Login(payload, checked).then((authResponse: AuthResponse) => {
                posthog.identify(authResponse.id.toString());
                posthog.capture('user_logged_in', {
                    userId: authResponse.id.toString(),
                    remember_me: checked
                });
                void navigate("/");
            }).catch(() => {
                setError("Error logging in. Please try again.");
                posthog.capture('user_login_failed');
            });
        }
        else
            setError("Please fill in all fields.");
    }

  return <div className="p-5 flex items-center justify-center min-h-screen pageBackground">
        <Title />
        <Form
            title="Login"
            size="medium"
            buttonText="Log in"
            error={error}
            submitHandler={submitHandler}
            addendum={<p className="noteText text-center">
                Don't have an account yet?
                <br/>
                <Link
                    className="underline hover:text-secondary"
                    to="/register"
                >
                    Register here
                </Link>
            </p>}
        >
            {/* Input fields */}
            <TextField
                id="email"
                type="email"
                label="E-mail"
                autoComplete="email"
                required
                onChange={(event) => setEmail(event.target.value)}
            />
            <TextField
                id="password"
                type="password"
                label="Password"
                autoComplete="current-password"
                required
                onChange={(event) => setPassword(event.target.value)}
            />

            {/* Remember me */}
            <div className="px-5 flex items-center justify-center">
                <Checkbox
                    checked={checked}
                    onChange={(event) => setChecked(event.target.checked)}
                    label="Remember me"
                />
            </div>
        </Form>
    </div>;
}
