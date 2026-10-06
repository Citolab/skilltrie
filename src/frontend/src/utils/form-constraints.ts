/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import type { Constraint } from "../components/general-components/textfield-mui";

export const validationConstraints: Record<string, Constraint[]> = {
    name: [
        {
            label: "Can only contain letters, hyphens, and apostrophes",
            verify: (v) => /^[a-z]+([ -'][a-z]+)*$/i.test(v),
        },
    ],
    displayName: [
        {
            label: "Can only contain letters, numbers, spaces, and hyphens",
            verify: (v) => /^(?=[^a-z^]*[a-z])[a-z0-9_\-ඞ]*$/i.test(v),
        },
        {
            label: "Has to contain at least 3 characters",
            verify: (v) => v.length >= 3,
        },
    ],
    email: [
        {
            label: "Must be valid e-mail",
            verify: (v) => /^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$/i.test(v),
        },
    ],
    infix: [
        {
            label: "Can only contain letters, hyphens, and apostrophes",
            verify: (v) => /^[a-z]+([ -'][a-z]+)*$/i.test(v),
        },
    ],
    password: [
        {
            label: "Must be at least 8 characters long",
            verify: (v) => v.length >= 8,
            display: true,
        },
        {
            label: "Must include an uppercase letter",
            verify: (v) => /[A-Z]/.test(v),
            display: true,
        },
        { label: "Must include a lowercase letter", verify: (v) => /[a-z]/.test(v), display: true },
        { label: "Must include a number", verify: (v) => /[0-9]/.test(v), display: true },
        {
            label: "Must include a special character",
            verify: (v) => /[!@#$%^&*(),.?":{}|<>-]/.test(v),
            display: true,
        },
    ],
};
