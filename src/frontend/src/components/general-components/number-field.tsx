/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { TextField, type SxProps, type Theme } from "@mui/material";

interface NumberFieldProps {
    /** Current numeric value; `undefined`/`NaN` renders an empty field. */
    value?: number;
    /** Called with the new value on edit; receives `NaN` when the field is cleared. */
    setValue?: (value: number) => void;
    /** Extra MUI styles merged into the underlying TextField. */
    sx?: SxProps<Theme>;
    /** MUI input size; defaults to "medium" when omitted. */
    size?: "small" | "medium";
    /** Stretches the field to its container width. */
    fullWidth?: boolean;
}

/**
 * Numeric text input restricted to positive whole numbers (minimum 1).
 *
 * Negatives are blocked at three layers: the "-" key is rejected, the native `min`
 * is 1, and entered values are clamped via `Math.max(1, …)`. Clearing the field is
 * still allowed and propagates `NaN` so callers can detect an empty input.
 */
export function NumberField(props: NumberFieldProps) {
    return (
        <TextField
            type={"number"}
            size={props.size}
            fullWidth={props.fullWidth}
            value={props.value}
            slotProps={{ htmlInput: { min: 1 } }}
            onChange={(e) => {
                if (!props.setValue) return;
                const parsed = parseInt(e.target.value);
                // Keep NaN so the field can be cleared, but never allow a value below 1.
                props.setValue(isNaN(parsed) ? parsed : Math.max(1, parsed));
            }}
            onKeyDown={(e) => {
                // No "-" in the allowed set, so negative numbers can't be typed.
                if (
                    !/[\d.]/.test(e.key) &&
                    !["Backspace", "Delete", "ArrowLeft", "ArrowRight", "Tab"].includes(e.key)
                ) {
                    e.preventDefault();
                }
            }}
            sx={{
                color: "var(--color-nav-text-LD)",
                borderColor: "var(--color-nav-text-LD)",
                ...props.sx,
            }}
        />
    );
}
