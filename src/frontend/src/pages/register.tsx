/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

/**
 *
 * Register form used purely for user registration from outside. If the registration succeeds, it automatically logs the user in.
 * Not used for creating users through the user dashboard (while the functionality is basically the same), that is handled differently.
 * @remarks .NET Identity automatically handles hashing of passwords, so passing a plaintext password here is fine.
 *
 * @returns
 *
 */

import { Link, useNavigate } from "react-router-dom";
import { useState } from "react";
import Form from "../components/general-components/form";
import TextField from "../components/general-components/textfield-mui";
import { RegisterApi, type AuthResponse, type RegisterPayload as Payload } from "../api/auth";
import { usePostHog } from "posthog-js/react";
import type { ServerError } from "../types/error";
import { SetErrorText } from "../utils/api-error";
import Checkbox from "../components/login/form/checkbox";
import { Title } from "../components/general-components/title.tsx";
import Modal from "@mui/material/Modal";
import Fade from "@mui/material/Fade";
import Box from "@mui/material/Box";
import { PrivacyStatementContent } from "../pages/privacy-statement.tsx";
import GeneralButton from "../components/general-components/general-button.tsx";
import { validationConstraints } from "../utils/form-constraints.ts";

export default function Register() {
    const [firstName, setFirstName] = useState<string>("");
    const [infix, setInfix] = useState<string>("");
    const [lastName, setLastName] = useState<string>("");
    const [displayName, setDisplayName] = useState<string>("");
    const [email, setEmail] = useState<string>("");
    const [password, setPassword] = useState<string>("");
    const [confirmPassword, setConfirmPassword] = useState<string>("");
    const [checked, setChecked] = useState<boolean>(false);
    const [confirmError, setConfirmError] = useState<boolean>(false);
    const [error, setError] = useState<string>("");
    const [showPrivacy, setShowPrivacy] = useState<boolean>(false);
    const navigate = useNavigate();
    const posthog = usePostHog();

    async function submitHandler(success: boolean) {
        if (success) {
            const payload: Payload = {
                firstName: firstName.trim(),
                infix: !!infix ? infix.trim() : null,
                lastName: lastName.trim(),
                displayName: displayName.trim(),
                email: email.trim(),
                password: password,
            };

            RegisterApi(payload)
                .then((authResponse: AuthResponse) => {
                    posthog.identify(authResponse.id.toString());
                    posthog.capture("user_registered", {
                        userId: authResponse.id.toString(),
                    });
                    void navigate("/");
                })
                .catch((error: ServerError) => SetErrorText(error, setError));
        } else setError("Please fill in all fields.");
    }

    return (
        <div className="p-5 flex items-center justify-center min-h-screen pageBackground">
            <Title />
            <Form
                title="Register"
                size="medium"
                buttonText="Register"
                error={error}
                submitHandler={submitHandler}
                addendum={
                    <p className="noteText text-center">
                        Already have an account?
                        <br />
                        <Link className="underline hover:text-secondary" to="/login">
                            Log in here
                        </Link>
                    </p>
                }
            >
                {/* Input fields */}
                <TextField
                    name="firstName"
                    type="text"
                    label="First Name"
                    autoComplete="given-name"
                    required
                    onChange={(event) => setFirstName(event.target.value)}
                    constraints={validationConstraints.name}
                />
                <TextField
                    name="infix"
                    type="text"
                    label="Infix"
                    autoComplete="additional-name"
                    onChange={(event) => setInfix(event.target.value)}
                    constraints={validationConstraints.name}
                />
                <TextField
                    name="lastName"
                    type="text"
                    label="Last Name"
                    autoComplete="family-name"
                    required
                    onChange={(event) => setLastName(event.target.value)}
                    constraints={validationConstraints.name}
                />
                <TextField
                    name="displayName"
                    type="text"
                    label="Display Name"
                    required
                    onChange={(event) => setDisplayName(event.target.value)}
                    constraints={validationConstraints.displayName}
                    maxLength={24}
                />
                <TextField
                    name="email"
                    type="email"
                    label="E-mail"
                    autoComplete="email"
                    required
                    onChange={(event) => setEmail(event.target.value)}
                    constraints={validationConstraints.email}
                />
                <TextField
                    name="password"
                    type="password"
                    label="Password"
                    autoComplete="new-password"
                    required
                    onChange={(event) => {
                        const value = event.target.value;
                        setPassword(value);
                        setConfirmError(confirmPassword.length > 0 && value !== confirmPassword);
                    }}
                    constraints={validationConstraints.password}
                />
                <TextField
                    name="confirmPassword"
                    type="password"
                    label="Confirm Password"
                    autoComplete="new-password"
                    required
                    error={confirmError}
                    onChange={(event) => {
                        let value = event.target.value;
                        setConfirmPassword(value);
                        setConfirmError(value.length > 0 && password !== value);
                    }}
                    constraints={[
                        {
                            verify: (input) => password === input,
                            label: "Must match password",
                        },
                    ]}
                />

                {/* Privacy statement */}
                <div className="px-5 flex items-center justify-center">
                    <Checkbox
                        checked={checked}
                        onChange={(event) => setChecked(event.target.checked)}
                        label={
                            <p className="text-center">
                                I have read and agree to the{" "}
                                <button
                                    type="button"
                                    onClick={() => setShowPrivacy(true)}
                                    className="underline hover:text-secondary"
                                >
                                    privacy statement
                                </button>
                            </p>
                        }
                        required
                    />
                </div>

                {/* privacy modal */}
                <Modal open={showPrivacy} onClose={() => setShowPrivacy(false)}>
                    <Fade in={showPrivacy}>
                        <Box
                            sx={{
                                position: "absolute",
                                top: "50%",
                                left: "50%",
                                transform: "translate(-50%, -50%)",
                                width: 500,
                                maxWidth: "90vw",
                                bgcolor: "background.paper",
                                borderRadius: 2,
                                boxShadow: 24,
                                p: 4,
                            }}
                        >
                            <div className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 bg-white rounded-lg p-6 max-w-lg w-full max-h-[80vh] overflow-y-auto">
                                <PrivacyStatementContent />
                                <div className="flex items-center justify-center">
                                    <div className="w-2/3">
                                        <GeneralButton
                                            type="button"
                                            variant="primary"
                                            size="medium"
                                            fullWidth={true}
                                            onClick={() => setShowPrivacy(false)}
                                        >
                                            Close
                                        </GeneralButton>
                                    </div>
                                </div>
                            </div>
                        </Box>
                    </Fade>
                </Modal>
            </Form>
        </div>
    );
}
