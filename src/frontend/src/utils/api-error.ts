/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import type { ApiError } from "../types/error";
import posthog from "posthog-js";
import type { Dispatch, SetStateAction } from "react";

export function ParseApiError(raw: string): ApiError | null {
    try {
        const jsonPart = raw.substring(raw.indexOf("{"));
        return JSON.parse(jsonPart) as ApiError;
    } catch {
        return null;
    }
}

export function GetErrorText(detail: string): string {
    switch (detail) {
        case "email_in_use":
            return "Email already in use, try a different email.";

        case "name_in_use":
            return "The chosen display name is already in use, try a different one.";

        default:
            return "An error occurred whilst registering.";
    }
}

export function SetErrorText(error: any, setError: Dispatch<SetStateAction<string>>) {
    const errorJSON: ApiError | null = ParseApiError(error.error);
    if (errorJSON) setError(GetErrorText(errorJSON?.detail));
    posthog.capture("user_registration_failed", {
        error: errorJSON?.detail ?? "unknown",
    });
}
