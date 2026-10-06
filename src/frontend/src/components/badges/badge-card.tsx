/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import type { BadgeInfo } from "@/types/badge-admin";
import { parseStampImage } from "@/utils/stamp-options";
import { Button, Card, CardContent, TextField } from "@mui/material";
import { DateTime } from "luxon";
import { useState } from "react";
import { LabeledField } from "./labeled-input";
import type { BadgeProps } from "@/pages/admin/badge-settings.tsx";

const textFieldStyling = {
    "& .MuiInputLabel-root": { color: "var(--color-nav-text-LD)" },
    "& .MuiInputBase-input": {
        color: "var(--color-nav-text-LD)",
        colorScheme: "light dark",
    },
    "& .MuiOutlinedInput-notchedOutline": {
        borderColor: "var(--color-admin-border--inactive)",
    },
    "&:hover .MuiOutlinedInput-notchedOutline": { borderColor: "var(--color-primary)" },
    "& .MuiOutlinedInput-root.Mui-focused .MuiOutlinedInput-notchedOutline": {
        borderColor: "var(--color-primary)",
    },
};

/**
 * Converts an ISO timestamp into the `yyyy-MM-ddTHH:mm` string a native
 * `datetime-local` input expects, in the browser's local time zone.
 *
 * @param iso - ISO date string from the API (or undefined).
 * @returns The formatted local value, or "" when the input is missing/invalid.
 */
function toDateTimeLocal(iso?: string) {
    if (!iso) return "";
    const parsed = DateTime.fromISO(iso);
    return parsed.isValid ? parsed.toFormat("yyyy-MM-dd'T'HH:mm") : "";
}

/**
 * Displays a single badge as a card (image, name, category, description, availability).
 *
 * When `staging` is true the availability fields are editable and the Save / Cancel /
 * Publish actions are shown; otherwise the card is read-only. The availability dates are
 * kept in local state so they can be edited before saving.
 *
 * @param badge - The badge to render.
 * @param staging - Whether this badge is in the editable staging phase.
 * @param onPublish - Invoked when the Publish button is clicked.
 * @param onRemove - Invoked when the Cancel button is clicked.
 * @param onSaveDates - Invoked with the edited availability window when Save is clicked.
 */
export function BadgeCard({
    badge,
    staging = false,
    onPublish,
    onRemove,
    onSave,
}: {
    badge: BadgeInfo;
    staging?: boolean;
    onPublish?: () => void;
    onRemove?: () => void;
    onSave?: (identifier: string, badgeProps: BadgeProps) => void;
}) {
    const [openFrom, setOpenFrom] = useState(toDateTimeLocal(badge.openFrom));
    const [openUntil, setOpenUntil] = useState(toDateTimeLocal(badge.openUntil));
    const [flagKey, setFlagKey] = useState<string | undefined>(badge.flagKey);
    const [flagVariant, setFlagVariant] = useState<string | undefined>(badge.flagVariant);

    // The backend requires openFrom < openUntil; the fixed format sorts chronologically.
    const canSaveDates =
        (openFrom === "" || openFrom > "1900-01-01") &&
        (openUntil === "" || openUntil > "1900-01-01") &&
        (openFrom !== "" && openUntil !== "" ? openFrom < openUntil : true);

    const canEdit = staging;

    return (
        <Card
            sx={{
                backgroundColor: "var(--color-nav-LD)",
                color: "var(--color-nav-text-LD)",
                borderRadius: 3,
                border: "1px solid var(--color-borderDefault)",
            }}
            elevation={0}
        >
            <CardContent>
                <div className="flex gap-4">
                    <img
                        src={parseStampImage(badge.stamp)}
                        alt={badge.name ?? "Badge image"}
                        className="w-20 h-20 shrink-0 object-contain"
                    />
                    <div className="flex flex-col min-w-0">
                        <span className="text-lg font-semibold truncate">
                            {badge.name ?? "Unnamed badge"}
                        </span>
                        {badge.category && (
                            <span className="text-xs uppercase tracking-wide text-text-muted">
                                {badge.category}
                            </span>
                        )}
                        {badge.description && <p className="text-sm mt-1">{badge.description}</p>}
                    </div>
                </div>

                <div
                    className="
                        grid grid-cols-1 sm:grid-cols-2 gap-3 mt-4
                        [&_.MuiInputBase-input]:text-nav-text-LD
                        [&_.MuiSvgIcon-root]:text-nav-text-LD"
                >
                    <LabeledField
                        label={"Open From"}
                        info="Users can see and complete this badge only after this date and time"
                    >
                        <DateField value={openFrom} onChange={setOpenFrom} readOnly={!canEdit} />
                    </LabeledField>
                    <LabeledField
                        label={"Open Until"}
                        info="Users can see and complete this badge until this date and time"
                    >
                        <DateField value={openUntil} onChange={setOpenUntil} readOnly={!canEdit} />
                    </LabeledField>
                    <LabeledField
                        label={"Posthog Flag Key"}
                        info="The flag key of the intended feature flag in Posthog. Will be shown to all users if the flag key does not exist"
                    >
                        <TextField
                            value={flagKey}
                            onChange={(e) => setFlagKey(e.target.value)}
                            sx={{
                                ...textFieldStyling,
                                pointerEvents: !canEdit ? "none" : "auto",
                            }}
                            slotProps={{
                                htmlInput: { readonly: !canEdit },
                            }}
                        />
                    </LabeledField>
                    <LabeledField
                        label={"Posthog Flag Variant"}
                        info="Only the users which have this variant of the feature flag rolled out will be able to see and complete this badge"
                    >
                        <TextField
                            value={flagVariant}
                            onChange={(e) => setFlagVariant(e.target.value)}
                            sx={{
                                ...textFieldStyling,
                                pointerEvents: !canEdit ? "none" : "auto",
                            }}
                            slotProps={{
                                htmlInput: { readOnly: !canEdit },
                            }}
                        />
                    </LabeledField>
                </div>

                {staging && (
                    <div className="flex justify-end gap-2 mt-4">
                        <Button
                            variant="outlined"
                            disabled={!canSaveDates}
                            onClick={() =>
                                onSave?.(badge.identifier, {
                                    openFrom,
                                    openUntil,
                                    flagKey,
                                    flagVariant,
                                })
                            }
                            sx={{
                                textTransform: "none",
                                borderRadius: "9999px",
                                borderColor: "var(--color-primary)",
                                color: "var(--color-primary)",
                                "&:hover": {
                                    backgroundColor: "var(--color-primary)",
                                    borderColor: "var(--color-primary)",
                                    color: "var(--color-text-light)",
                                },
                            }}
                        >
                            Save
                        </Button>
                        <Button
                            variant="outlined"
                            color="error"
                            onClick={onRemove}
                            sx={{
                                textTransform: "none",
                                borderRadius: "9999px",
                                borderColor: "var(--color-error)",
                                color: "var(--color-error)",
                                "&:hover": {
                                    backgroundColor: "var(--color-error)",
                                    borderColor: "var(--color-error)",
                                    color: "var(--color-text-light)",
                                },
                            }}
                        >
                            Cancel
                        </Button>
                        <Button
                            variant="contained"
                            onClick={onPublish}
                            sx={{
                                textTransform: "none",
                                borderRadius: "9999px",
                                backgroundColor: "var(--color-primary)",
                                "&:hover": { backgroundColor: "var(--color-primary-dark)" },
                            }}
                        >
                            Publish
                        </Button>
                    </div>
                )}
            </CardContent>
        </Card>
    );
}

/**
 * Controlled `datetime-local` field themed for light/dark mode.
 *
 * @param label - Field label.
 * @param value - Current value as a `yyyy-MM-ddTHH:mm` string (see {@link toDateTimeLocal}).
 * @param onChange - Called with the new string value on edit.
 * @param readOnly - When true the field is display-only (used for published badges).
 */
function DateField({
    label,
    value,
    onChange,
    readOnly,
}: {
    label?: string;
    value: string;
    onChange?: (value: string) => void;
    readOnly?: boolean;
}) {
    return (
        <TextField
            label={label}
            type="datetime-local"
            value={value}
            onChange={(e) => onChange?.(e.target.value)}
            size="small"
            fullWidth
            slotProps={{
                inputLabel: { shrink: true },
                htmlInput: { readOnly },
            }}
            sx={{ ...textFieldStyling, pointerEvents: readOnly ? "none" : "auto" }}
        />
    );
}
