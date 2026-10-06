/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { useEffect, useState } from "react";
import type { ConstraintCheck } from "../admin-generic-object-form";
import RequiredAsterix from "../../general-components/required-asterix";

interface LabelledInputProps {
    /** type of input for different data types */
    type:
        | "text"
        | "small-text"
        | "boolean"
        | "number"
        | "dropdown"
        | "immutable"
        | "array"
        | "password"
        | "registerField"
        | "email";
    /** name of the label */
    label: string;
    /** placeholder */
    placeholder?: string;
    /** name attribute */
    name?: string;
    /** initial value -> it is the responsibility of the parent component to manage state */
    value?: React.InputHTMLAttributes<HTMLInputElement>["value"];
    /** callback function for when value has changed */
    onChange?: (value: unknown) => void;
    /** all possible values for the "dropdown" type -> ignored if type is not dropdown */
    options?: string[];
    /** whether the input is disabled */
    disabled?: boolean;
    /** a list of error strings to show -> if empty, nothing is shown */
    errors?: ConstraintCheck[];
    /** what type of autocomplete to use */
    autocomplete?: string;
    /** whether it is required or not */
    required?: boolean;
}

function LabelledInput(props: LabelledInputProps) {
    /** Whether the input field is currently selected or not. At the start of the page the value is null and denotes that the field has not been touched yet */
    const [touchedState, setTouchedState] = useState<boolean | null>(null);
    const hasErrors = props.errors?.some((check) => check.isError) ?? false;

    const baseStyles = `w-full border rounded bg-gray-300 dark:bg-gray-700
         focus-within:ring-2
         text-gray-900 
         dark:text-white 
         placeholder-gray-400 
         ${
             hasErrors
                 ? "border border-red-500 focus-within:ring-red-500 "
                 : "border border-gray-600 focus-within:ring-primary "
         }
        `;
    const baseRegisterStyles = `w-full border-3 rounded-2xl bg-transparent
    focus-within:ring-2
    text-black
    placeholder-gray-900
    ${
        touchedState
            ? "border border-blue-500 focus-within:ring-blue-500"
            : hasErrors && touchedState != null
              ? "border border-red-400"
              : "border border-gray-400"
    }   
    `;

    const inputStyles = "w-full px-3 py-2 text-sm outline-none bg-inherit";
    const registerStyles = "w-full px-3 py-2 text-sm outline-none bg-inherit placeholder-gray-600";
    const labelStyles = "text-black";

    // TODO: remove -> In conflict with the practice that the parent component
    // should manage state, too lazy to remove this now
    const [numericState, setNumericState] = useState<number | null>(Number(props.value));
    const [booleanState, setBooleanState] = useState<boolean | null>(Boolean(props.value));

    useEffect(() => {
        console.log(props.value);
        props.onChange?.(props.value);
    }, []);

    function renderInput() {
        switch (props.type) {
            case "password":
                return (
                    <>
                        <label
                            htmlFor={props.name}
                            className={`flex items-center gap-1 ${labelStyles}`}
                        >
                            <span>{props.label}</span>
                            {props.required && <RequiredAsterix></RequiredAsterix>}
                        </label>
                        <div className={`flex item-center gap-1 ${baseRegisterStyles}`}>
                            <input
                                type="password"
                                className={`${registerStyles}`}
                                placeholder={props.placeholder}
                                name={props.name}
                                id={props.name}
                                value={props.value}
                                disabled={props.disabled}
                                autoComplete={props.autocomplete}
                                onFocus={() => setTouchedState(true)}
                                onBlur={() => {
                                    if (touchedState) setTouchedState(false);
                                }}
                                onChange={(e) => props.onChange?.(e.target.value)}
                            />
                        </div>
                    </>
                );
            case "registerField":
                return (
                    <>
                        <label
                            htmlFor={props.name}
                            className={`flex items-center gap-1 ${labelStyles}`}
                        >
                            <span>{props.label}</span>
                            {props.required && <RequiredAsterix></RequiredAsterix>}
                        </label>
                        <div className={`flex items-center gap-1 ${baseRegisterStyles}`}>
                            <input
                                type="text"
                                className={`${registerStyles}`}
                                placeholder={props.placeholder}
                                name={props.name}
                                value={props.value}
                                disabled={props.disabled}
                                autoComplete={props.autocomplete}
                                onFocus={() => setTouchedState(true)}
                                onBlur={() => {
                                    if (touchedState) setTouchedState(false);
                                }}
                                onChange={(e) => {
                                    props.onChange?.(e.target.value);
                                }}
                            />
                        </div>
                    </>
                );
            case "email":
                return (
                    <>
                        <label htmlFor={props.name} className="flex items-center gap-1">
                            <span>{props.label}</span>
                            {props.required && <RequiredAsterix></RequiredAsterix>}
                        </label>
                        <div className={`flex items-center gap-1 ${baseRegisterStyles}`}>
                            <input
                                type="email"
                                className={`${registerStyles}`}
                                placeholder={props.placeholder}
                                name={props.name}
                                value={props.value}
                                disabled={props.disabled}
                                autoComplete={props.autocomplete}
                                onFocus={() => setTouchedState(true)}
                                onBlur={() => {
                                    if (touchedState) setTouchedState(false);
                                }}
                                onChange={(e) => {
                                    props.onChange?.(e.target.value);
                                }}
                            />
                        </div>
                    </>
                );
            case "array":
                return (
                    <div className={`flex items-center ${baseStyles}`}>
                        <input
                            type="text"
                            className={`${inputStyles}`}
                            placeholder={props.placeholder}
                            name={props.name}
                            value={props.value}
                            disabled={props.disabled}
                            autoComplete={props.autocomplete}
                            onChange={(e) => props.onChange?.(String(e.target.value).split(","))}
                        />
                    </div>
                );
            case "text":
                return (
                    <div className={`flex items-start ${baseStyles}`}>
                        <textarea
                            className={`${inputStyles} resize-none min-h-[200px]`}
                            placeholder={props.placeholder}
                            name={props.name}
                            value={props.value}
                            disabled={props.disabled}
                            autoComplete={props.autocomplete}
                            onChange={(e) => props.onChange?.(e.target.value)}
                        />
                    </div>
                );

            case "small-text":
                return (
                    <div className={`flex items-center ${baseStyles}`}>
                        <input
                            type="text"
                            className={`${inputStyles}`}
                            placeholder={props.placeholder}
                            name={props.name}
                            value={props.value}
                            disabled={props.disabled}
                            autoComplete={props.autocomplete}
                            onFocus={() => setTouchedState(true)}
                            onBlur={() => {
                                if (touchedState) setTouchedState(false);
                            }}
                            onChange={(e) => props.onChange?.(e.target.value)}
                        />
                    </div>
                );

            case "boolean":
                return (
                    <div className="flex items-center gap-2">
                        <button
                            type="button"
                            onClick={() => {
                                setBooleanState(!booleanState);
                                props.onChange?.(!booleanState);
                            }}
                            disabled={props.disabled}
                            className={`w-10 h-5 flex items-center rounded-full p-1 transition-colors duration-200 cursor-pointer
                                ${booleanState ? "bg-primary" : "bg-gray-300"}
                            `}
                        >
                            <span
                                className={`bg-white w-4 h-4 rounded-full shadow-md transform transition-transform duration-200
                                    ${booleanState ? "translate-x-5" : "translate-x-0"}
                                `}
                            />
                        </button>
                        <span className="text-sm text-gray-300">{props.placeholder}</span>
                    </div>
                );

            case "number":
                return (
                    <div className={`flex items-center ${baseStyles}`}>
                        <input
                            type="number"
                            step="1"
                            className={`${inputStyles}`}
                            placeholder={props.placeholder}
                            name={props.name}
                            value={numericState ?? props.value}
                            disabled={props.disabled}
                            autoComplete={props.autocomplete}
                            onChange={(e) => {
                                setNumericState(
                                    e.target.value === "" ? null : parseInt(e.target.value)
                                );
                                props.onChange?.(numericState);
                            }}
                        />
                    </div>
                );

            case "dropdown":
                return (
                    <div className={`flex items-center ${baseStyles}`}>
                        <select
                            className={`${inputStyles} cursor-pointer`}
                            value={props.value}
                            disabled={props.disabled}
                            onChange={(e) => props.onChange?.(e.target.value)}
                        >
                            <option value="" disabled>
                                {props.placeholder}
                            </option>
                            {props.options?.map((opt, index) => (
                                <option key={index} value={opt}>
                                    {opt}
                                </option>
                            ))}
                        </select>
                    </div>
                );

            case "immutable":
                return (
                    <div className={`flex items-center ${baseStyles}`}>
                        <input
                            type="text"
                            className={`${inputStyles} cursor-not-allowed bg-gray-600`}
                            value={props.value}
                            disabled
                        />
                    </div>
                );

            default:
                return null;
        }
    }

    return (
        <div className={"flex flex-col gap-1"}>
            {/* the label */}
            {!(props.type === "registerField" || props.type === "password") && props.label && (
                <label className="text-sm font-bold text-black dark:text-gray-300 first-letter:uppercase">
                    {props.label}
                </label>
            )}

            {/* the specific type of input */}
            {renderInput()}

            {/* the constraints belonging to password */}
            {props.name === "password" && (
                <div className="text-red-500 text-xs font-medium mt-1">
                    {props.errors?.map(
                        (check, index) =>
                            check.display && (
                                <p
                                    key={index}
                                    className={check.isError ? "text-error" : "text-success"}
                                >
                                    {check.constraint}
                                </p>
                            )
                    )}
                </div>
            )}
            {/* the constraints for others */}
            {props.name !== "password" && (
                <div className="text-red-500 text-xs font-medium mt-1">
                    {/* Only display error if component has been touched and there exist errors */}
                    {touchedState != null
                        ? (props.errors?.find((e) => e.isError && e.display)?.constraint ?? "")
                        : ""}
                </div>
            )}
        </div>
    );
}

export default LabelledInput;
