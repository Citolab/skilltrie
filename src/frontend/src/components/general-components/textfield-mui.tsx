/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

/**
 * Wrapper around MUI TextField with shared base styling.
 *
 * Common customizations are exposed as named props so you don't need to write
 * MUI selector strings yourself. For anything beyond these, pass an `sx` prop
 * and it will be merged in on top.
 *
 * See: https://mui.com/material-ui/api/text-field/
 *
 * @example
 * // Minimal usage — uses all defaults
 * <InputFieldMui label="Email" name="email" />
 *
 * @example
 * // Custom focus color and background
 * <InputFieldMui label="Email" backgroundColor="var(--color-gray-400)" focusBorderColor="var(--color-secondary)" />
 */

import { useState, type ReactNode, type CSSProperties, useEffect, useRef, useId } from "react";
import TextFieldMui, { type TextFieldProps as TextFieldMuiProps } from "@mui/material/TextField";
import Tooltip, { type TooltipProps } from "./tooltip-mui";
import IconButton from "@mui/material/IconButton";
import InputAdornment from "@mui/material/InputAdornment";
import { EyeIcon, EyeSlashIcon } from "@heroicons/react/24/outline";
import { CheckCircleIcon, XCircleIcon } from "@heroicons/react/24/solid";
import { useFormContext } from "./form";

export type Constraint = {
    /** Function that returns whether the input is conform this constraint or not. */
    verify: (input: string) => boolean;
    /** Description of the error. */
    label: string;
    /** Whether or not this error should be displayed at all times. */
    display?: boolean;
}

type ErrorMessage = {
    /** The id to designate the HTML table row. */
    id: number;
    /** Description of the error. */
    label: string;
    /** Whether or not this is an error or for display. */
    error: boolean;
}

export type TextFieldProps = TextFieldMuiProps & {
    /** Background color of the input box. */
    backgroundColor?: CSSProperties["color"];
    /** Border color when the field is focused. */
    focusBorderColor?: CSSProperties["color"];
    /** Border color when the field has an error. */
    errorBorderColor?: CSSProperties["color"];
    /** List of constraints the input should conform to. */
    constraints?: Constraint[];
    /** Maximum length of inputs */
    maxLength?: number;
    /** The properties of the tooltip. */
    tooltipProps?: TooltipProps;
}

export default function TextField({
    backgroundColor = "var(--color-background-light)",
    focusBorderColor = "var(--color-primary)",
    errorBorderColor = "var(--color-error)",
    constraints,
    maxLength = 100,
    tooltipProps,
    type = "text",
    ...props
}: TextFieldProps) {
    const [value, setValue] = useState<string>("");
    const [display, setDisplay] = useState<boolean>(false);
    const [focus, setFocus] = useState<boolean>(false);
    const [error, setError] = useState<boolean>(false);
    const [message, setMessage] = useState<ReactNode>(undefined);
    const form = useFormContext();
    const inputRef = useRef<HTMLInputElement>(null);
    const isPasswordField = type === "password";
    const id = useId();

    useEffect(() => {
        verify(value.trim());
    }, [props.error, value]);

    useEffect(() => {
        if (form?.submitCount && form.submitCount > 0)
            verify(value.trim(), true);
        }, [form?.submitCount]);
    
    function focusField() {
        requestAnimationFrame(() => {
            inputRef.current?.scrollIntoView?.({
                behavior: "smooth",
                block: "center",
                inline: "nearest"
            });
            setTimeout(() => {
                inputRef.current?.focus();
            }, 200);
        });
    }

    function verify(value: string, submit: boolean = false): void {
        const messages = getMessages(value);
        const anyError = props.error || messages.some((message) => message.error);
        const displayError = submit ? anyError : messages.some((message) => message.id !== -1 && message.error);
        setError(displayError);
        if (displayError)
            setMessage(getMessage(messages));
        form?.registerField(id, { validate:() => !anyError, focus:focusField });
    }

    function getMessages(value: string): ErrorMessage[] {
        let messages: ErrorMessage[] = [];

        if (!!value)
            constraints?.forEach((constraint, index) => {
                if (!constraint.verify(value))
                    messages.push({ id:index, label:constraint.label, error:true });
                else if (constraint.display)
                    messages.push({ id:index, label:constraint.label, error:false });
            });
        else if (props.required)
            messages.push({ id:-1, label:"This field is required", error:true });
        
        if (messages.length === 0 && props.error)
            messages.push({ id:-2, label:"This field does not conform the requirements", error:true });

        return messages;
    }

    function getMessage(messages: ErrorMessage[]): ReactNode {
        return <div className="flex flex-col items-center">
            <h2 className="font-bold text-text-primary">Error</h2>
            <table className="flex flex-col items-center border-separate [border-spacing:8px_0]">
                <tbody>
                    {messages.map((message) => <tr key={message.id}>
                        <td>{message.error ? <XCircleIcon className="size-5 text-error"/> : <CheckCircleIcon className="size-5 text-success"/>}</td>
                        <td>{message.label}</td>
                    </tr>)}
                </tbody>
            </table>
        </div>;
    }

    return <Tooltip
        borderColor="var(--color-error)"
        width={250}
        open={focus && error}
        placement="right"
        title={message}
        {...tooltipProps}
    >
        <TextFieldMui
            inputRef={inputRef}
            type={isPasswordField ? (display ? "text" : "password") : type}
            variant="outlined"
            size="small"
            fullWidth
            onFocus={() => setFocus(true)}
            onBlur={() => setFocus(false)}
            slotProps={{
                htmlInput: {
                    maxLength: maxLength
                },
                input: {
                    endAdornment: isPasswordField ? <InputAdornment position="end">
                        <IconButton
                            onClick={() => setDisplay((value) => !value)}
                            edge="end"
                            aria-label={display ? "Hide password" : "Show password"}
                        >
                            {display ? <EyeIcon className="size-5" /> : <EyeSlashIcon className="size-5" />}
                        </IconButton>
                    </InputAdornment> : undefined
                }
            }}
            {...props}
            sx={{
                // Apply the background color to the input root so it shows inside the border
                "& .MuiOutlinedInput-root": { backgroundColor },
                // Keep the label standard
                "& .MuiInputLabel-root": { color: "var(--color-text-dark)" },
                "& .MuiInputLabel-root.Mui-focused": { color: "var(--color-text-dark)" },
                "& .MuiInputLabel-root.Mui-error": { color: "var(--color-text-dark)" },
                "& .MuiInputLabel-asterisk": { color: "var(--color-error)" },
                "& .MuiInputLabel-asterisk.Mui-error": { color: "var(--color-error)" },
                // Override MUI's default blue focus ring with the app's primary color
                "& .MuiOutlinedInput-root.Mui-focused .MuiOutlinedInput-notchedOutline": {
                    borderColor: focusBorderColor,
                },
                // Override MUI's default red error ring with the app's error color
                "& .MuiOutlinedInput-root.Mui-error .MuiOutlinedInput-notchedOutline": {
                    borderColor: errorBorderColor,
                },
                // Browsers override both background color and font on autofill via :-webkit-autofill.
                // box-shadow inset covers the background; font properties restore the app's font.
                "& input:-webkit-autofill": {
                    WebkitBoxShadow: `0 0 0 1000px ${backgroundColor} inset`,
                    fontFamily: "inherit",
                    fontSize: "inherit"
                },
                // Caller's sx is merged last so it can override anything above
                ...props.sx
            }}
            error={error}
            onChange={(event) => {
                setValue(event.target.value);
                props.onChange?.(event);
            }}
        />
    </Tooltip>;
}
