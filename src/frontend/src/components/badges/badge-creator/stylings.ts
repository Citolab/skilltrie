/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

// MUI has no ThemeProvider in this app, so its components render in the default
// light palette even in dark mode. Drive their colours from the app's CSS variables
// instead (these flip automatically via the prefers-color-scheme media query).
import type { StylesConfig } from "react-select";
import type { TopicLabel } from "@/types/topic.ts";

export const fieldSx = {
    // Text colour — plain inputs/selects and the x-date-pickers "sections" field.
    "& .MuiInputBase-input, & .MuiSelect-select, & .MuiPickersInputBase-root, & .MuiPickersSectionList-root":
        { color: "var(--color-nav-text-LD)" },
    "& .MuiSvgIcon-root": { color: "var(--color-nav-text-LD)" },
    // Resting border — plain inputs use MuiOutlinedInput, pickers use MuiPickersOutlinedInput.
    "& .MuiOutlinedInput-notchedOutline, & .MuiPickersOutlinedInput-notchedOutline": {
        borderColor: "var(--color-admin-border--inactive)",
    },
    // Hover border — cover both the standalone Select (root === &) and the TextField
    // (root is a descendant of the FormControl &); the descendant form outranks MUI's default.
    "&:hover .MuiOutlinedInput-notchedOutline, & .MuiOutlinedInput-root:hover .MuiOutlinedInput-notchedOutline, &:hover .MuiPickersOutlinedInput-notchedOutline, & .MuiPickersOutlinedInput-root:hover .MuiPickersOutlinedInput-notchedOutline":
        { borderColor: "var(--color-primary)" },
    // Focus border — same dual coverage for Select and TextField.
    "&.Mui-focused .MuiOutlinedInput-notchedOutline, & .MuiOutlinedInput-root.Mui-focused .MuiOutlinedInput-notchedOutline, & .MuiPickersOutlinedInput-root.Mui-focused .MuiPickersOutlinedInput-notchedOutline":
        { borderColor: "var(--color-primary)" },
};

// Dropdown popups render in a portal outside the themed container, so colour the
// menu surface explicitly to keep it consistent in light and dark mode.
export const dropdownMenuProps = {
    slotProps: {
        paper: {
            sx: {
                backgroundColor: "var(--color-nav-LD)",
                color: "var(--color-nav-text-LD)",
                "& .MuiMenuItem-root:hover": { backgroundColor: "var(--color-nav-LD--hover)" },
                "& .MuiMenuItem-root.Mui-selected": {
                    backgroundColor: "var(--color-nav-active-LD)",
                },
            },
        },
    },
};

// Styles the react-select topic picker to match the MUI outlined inputs (small size,
// neutral resting border that turns primary on hover/focus), all from the same CSS vars.
export const topicSelectStyles: StylesConfig<TopicLabel, false> = {
    control: (base, state) => ({
        ...base,
        minHeight: 40,
        backgroundColor: "transparent",
        borderRadius: 4,
        boxShadow: "none",
        borderColor: state.isFocused
            ? "var(--color-primary)"
            : "var(--color-admin-border--inactive)",
        "&:hover": { borderColor: "var(--color-primary)" },
    }),
    valueContainer: (base) => ({ ...base, padding: "0 14px" }),
    input: (base) => ({ ...base, margin: 0, color: "var(--color-nav-text-LD)" }),
    singleValue: (base) => ({ ...base, color: "var(--color-nav-text-LD)" }),
    placeholder: (base) => ({ ...base, color: "var(--color-text-muted)" }),
    dropdownIndicator: (base) => ({ ...base, color: "var(--color-nav-text-LD)" }),
    // Hidden so the topic picker matches the MUI selects, which have no separator line.
    indicatorSeparator: () => ({ display: "none" }),
    menu: (base) => ({ ...base, backgroundColor: "var(--color-nav-LD)", zIndex: 20 }),
    option: (base, state) => ({
        ...base,
        backgroundColor: state.isFocused ? "var(--color-nav-LD--hover)" : "transparent",
        color: "var(--color-nav-text-LD)",
    }),
};
