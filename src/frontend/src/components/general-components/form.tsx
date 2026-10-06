/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import React, { useState, createContext, useContext, type ReactNode, useRef } from "react";
import Button from "./general-button";

type FieldActions = {
    validate: () => boolean,
    focus: () => void
}
type FormContextType = {
  submitCount: number;
  registerField: (id: string, field: FieldActions) => void;
}
const FormContext = createContext<FormContextType | null>(null)
export const useFormContext = () => useContext(FormContext)

export type FormProps = {
    /** The children of the object */
    children?: ReactNode;
    /** The title to display */
    title?: string;
    /** Background color of the form. Defaults to the app's --color-background-light. */
    backgroundColor?: "bg-background-light" | "bg-background-dark";
    /** The size of the form */
    size?: "small" | "medium" | "large" | "full"
    /** The text displayed on the submit button */
    buttonText?: string;
    /** The error to display */
    error?: string;
    /** Handler to call when form is submitted */
    submitHandler?: (success: boolean) => void;
    /** Extra information to display on the bottom of the form */
    addendum?: ReactNode;
}

export default function Form({
    children,
    title,
    backgroundColor = "bg-background-light",
    size = "full",
    buttonText = "Submit",
    error,
    submitHandler,
    addendum
}: FormProps) {
    const [submitCount, setSubmitCount] = useState(0);
    const fieldsRef = useRef(new Map<string, FieldActions>());
    const registerField = (id: string, field: FieldActions) => fieldsRef.current.set(id, field);
    const outerMargin = "p-6"; // Minimum vertical distance between form and elements, horizontal is doubled
    const elementGap = "gap-3"; // Minimum vertical distance between elements
    const childGap = "gap-2"; // Minimum vertical distance between children

    const sizes = {
        small: "w-60",
        medium: "w-80",
        large: "w-100",
        xlarge: "w-120",
        full: "w-full"
    };

    return <form
        className={`${outerMargin} ${sizes[size]} ${backgroundColor} rounded-xl flex flex-col ${elementGap} items-center justify-center`}
        noValidate
        onSubmit={(event) => {
            event.preventDefault();
            setSubmitCount((value) => value + 1);
            const errors = Array.from(fieldsRef.current.values()).filter((field) => !field.validate());
            const anyError = errors.length > 0;
            if (anyError)
                errors[0].focus();
            submitHandler?.(!anyError);
        }}
    >
        {/* Insert title if there is one */}
        {title && <h1 className={`titleText text-center`}>
            {title}
        </h1>}

        {/* Insert input fields */}
        {React.Children.count(children) > 0 && <div className={`w-full flex flex-col ${childGap}`}>
            <FormContext.Provider
                value={{
                    submitCount,
                    registerField
                }}
            >
                {children}
            </FormContext.Provider>
        </div>}

        {/* Temporary display of backend errors, this should be replaced with a more UI friendly alternative (such as adding a tooltip to the button as in the commented code below) */}
        {error && <p className="text-error">
            {error}
        </p>}

        {/* Submit button */}
        <div className="w-2/3">
            <Button
                id="submit"
                type="submit"
                variant="primary"
                fullWidth
            >
                {buttonText}
            </Button>
        </div>

        {/* Extra information to display */}
        {addendum}
    </form>
}