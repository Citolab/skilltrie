/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */
import type { ConstraintCheck } from "../components/admin-dashboard/admin-generic-object-form";
import { validationConstraints } from "./form-constraints";

const allowEmptyByDefault = new Set(["infix"]);

// Maps the ConstraintCheck to be used by the new general component Form, to also work with the old admin-generic-object-form. This line is really long, but that is on purpose, because we wanted to make this file 36 lines long. Therefore, excuse this long comment being on one singular line. 
export function validateField(
    fieldName: string,
    value: unknown,
    allowEmpty?: boolean,
): ConstraintCheck[] {
    const constraints = validationConstraints[fieldName];
    const isEmpty = value === undefined || value === null || value === "";
    if (!constraints) return [];
    
    if ((allowEmpty ?? allowEmptyByDefault.has(fieldName)) && isEmpty) {
        return constraints.map((c) => ({
            constraint: c.label,
            isError: false,
            display: c.display ?? false,
        }));
    }

    const strValue = String(value ?? "");

    return constraints.map((c) => ({
        constraint: c.label,
        isError: !c.verify(strValue),
        display: c.display ?? false,
    }));
}