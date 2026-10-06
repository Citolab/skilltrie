/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { type ReactElement, useEffect, useReducer, useState } from "react";
import type { ParameterDTO, ParameterizedBadges } from "@/types/badge-admin.ts";
import { AddParameterizedBadge, GetParameterizedBadges } from "@/api/badge-admin.ts";
import {
    Accordion,
    AccordionDetails,
    Button,
    MenuItem,
    Select,
    type SelectChangeEvent,
    TextField,
} from "@mui/material";
import { AdapterLuxon } from "@mui/x-date-pickers/AdapterLuxon";
import { LocalizationProvider } from "@mui/x-date-pickers";
import { stamps } from "@/utils/stamp-options.ts";
import AccordionSummary from "@mui/material/AccordionSummary";
import ExpandMoreIcon from "@mui/icons-material/ExpandMore";
import { toast } from "react-toastify";
import { InfoTooltip, LabeledField } from "@/components/badges/labeled-input.tsx";
import { dropdownMenuProps, fieldSx } from "@/components/badges/badge-creator/stylings.ts";
import {
    FromNameMapping,
    InputRegistry,
    OverwriteMeta,
    ParameterMeta,
} from "@/components/badges/badge-creator/mappings.tsx";

/**
 * Reducer backing the form's field values: a flat `{ key: value }` map keyed by
 * parameter type or overwrite property name. Returns a new object on each update so
 * React re-renders.
 */
const inputReducer = (inputs: Record<string, unknown>, action: { key: string; value: unknown }) => {
    const { key, value } = action;
    const newInputs = { ...inputs };
    newInputs[key] = value;
    return newInputs;
};

/**
 * Admin form for creating a parameterized badge. The admin picks a badge type, fills in
 * its required parameter(s), and may overwrite default properties (name/description/
 * category/stamp). On submit the badge is created in the staging phase.
 *
 * @param onCreated - Called after a badge is successfully staged (e.g. to refresh a list).
 */
export function BadgeCreator({ onCreated }: { onCreated?: () => void }) {
    const [badgeTypes, setBadgeTypes] = useState<ParameterizedBadges>();
    const [selectedBadgeType, setSelectedBadgeType] = useState<string | null>(null);
    const [inputs, dispatchInputs] = useReducer(inputReducer, {});

    function handleSubmit() {
        AddParameterizedBadge({ ...inputs, BadgeImage: selectedBadgeType })
            .then(() => {
                toast.success("Parameterized badge successfully added");
                onCreated?.();
            })
            .catch(() =>
                toast.error(
                    "Failed to add parameterized badge. Maybe this kind of badge already exists?"
                )
            );
    }

    useEffect(() => {
        void GetParameterizedBadges().then((b) => {
            setBadgeTypes(b);
            if (Object.keys(b).length > 0) setSelectedBadgeType(Object.keys(b)[0]);
        });
    }, []);

    return (
        <section className="mb-10 text-nav-text-LD">
            <h2 className="text-2xl font-title font-semibold text-text-primary mb-1 ">
                Create a badge
                <span className="ml-1">
                    <InfoTooltip
                        text="You can create badges of given types with different parameters.
                          Multiple badges of the same type are allowed as long as the parameters are different."
                    />
                </span>
            </h2>
            <hr className="border-borderDefault mb-4" />

            <div
                className="
                    max-w-2xl mx-auto flex flex-col gap-6
                    bg-nav-LD border border-borderDefault rounded-2xl shadow-md p-6
                    [&_.MuiInputBase-input]:text-nav-text-LD
                    [&_.MuiInputBase-root]:text-nav-text-LD
                    [&_.MuiSvgIcon-root]:text-nav-text-LD
                "
            >
                <LabeledField
                    label="Badge type"
                    info="The kind of badge to create. Each kind requires its own parameters below."
                >
                    <Select
                        value={selectedBadgeType ?? ""}
                        onChange={(event: SelectChangeEvent<string>) => {
                            setSelectedBadgeType(event.target.value);
                        }}
                        size="small"
                        fullWidth
                        sx={fieldSx}
                        MenuProps={dropdownMenuProps}
                        IconComponent={ExpandMoreIcon}
                    >
                        {Object.keys(badgeTypes ?? {}).map((badgeType) => (
                            <MenuItem key={badgeType} value={badgeType}>
                                {FromNameMapping(badgeType)}
                            </MenuItem>
                        ))}
                    </Select>
                </LabeledField>

                <form className="flex flex-col gap-5">
                    {badgeTypes && selectedBadgeType && (
                        <LocalizationProvider dateAdapter={AdapterLuxon}>
                            {badgeTypes[selectedBadgeType].map((p: ParameterDTO): ReactElement => {
                                const meta = ParameterMeta[p.name];
                                return (
                                    <LabeledField
                                        key={p.type}
                                        label={meta?.label ?? p.name}
                                        info={meta?.description}
                                    >
                                        {InputRegistry[p.type]?.({
                                            value: inputs[p.type],
                                            setValue: (value) => {
                                                dispatchInputs({ key: p.type, value: value });
                                            },
                                        })}
                                    </LabeledField>
                                );
                            })}
                        </LocalizationProvider>
                    )}

                    <Accordion
                        disableGutters
                        sx={{
                            backgroundColor: "transparent",
                            color: "var(--color-nav-text-LD)",
                            boxShadow: "none",
                            border: "1px solid var(--color-borderDefault)",
                            borderRadius: "0.75rem",
                            "&:before": { display: "none" },
                        }}
                    >
                        <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                            <span className="flex items-center gap-1">
                                Overwrite values
                                <InfoTooltip text="Optionally overwrite the default properties generated for this badge." />
                            </span>
                        </AccordionSummary>
                        <AccordionDetails>
                            <div className="flex flex-col gap-5">
                                <LabeledField
                                    label={OverwriteMeta.name.label}
                                    info={OverwriteMeta.name.description}
                                >
                                    <TextField
                                        value={inputs["name"] ?? ""}
                                        onChange={(e) =>
                                            dispatchInputs({ key: "name", value: e.target.value })
                                        }
                                        size="small"
                                        fullWidth
                                        sx={fieldSx}
                                    />
                                </LabeledField>

                                <LabeledField
                                    label={OverwriteMeta.description.label}
                                    info={OverwriteMeta.description.description}
                                >
                                    <TextField
                                        value={inputs["description"] ?? ""}
                                        onChange={(e) =>
                                            dispatchInputs({
                                                key: "description",
                                                value: e.target.value,
                                            })
                                        }
                                        size="small"
                                        fullWidth
                                        multiline
                                        minRows={2}
                                        sx={fieldSx}
                                    />
                                </LabeledField>

                                <LabeledField
                                    label={OverwriteMeta.category.label}
                                    info={OverwriteMeta.category.description}
                                >
                                    <TextField
                                        value={inputs["category"] ?? ""}
                                        onChange={(e) =>
                                            dispatchInputs({
                                                key: "category",
                                                value: e.target.value,
                                            })
                                        }
                                        size="small"
                                        fullWidth
                                        sx={fieldSx}
                                    />
                                </LabeledField>

                                <LabeledField
                                    label={OverwriteMeta.stamp.label}
                                    info={OverwriteMeta.stamp.description}
                                >
                                    <Select
                                        value={(inputs["stamp"] as string) ?? ""}
                                        onChange={(e) =>
                                            dispatchInputs({
                                                key: "stamp",
                                                value: e.target.value || undefined,
                                            })
                                        }
                                        size="small"
                                        fullWidth
                                        displayEmpty
                                        sx={fieldSx}
                                        MenuProps={dropdownMenuProps}
                                        IconComponent={ExpandMoreIcon}
                                    >
                                        <MenuItem value="">None</MenuItem>
                                        {stamps.map((stamp) => (
                                            <MenuItem key={stamp.name} value={stamp.name}>
                                                <span className="flex items-center gap-2">
                                                    <img
                                                        src={stamp.path}
                                                        alt={stamp.name}
                                                        className="w-8 h-8"
                                                    />
                                                    {stamp.name}
                                                </span>
                                            </MenuItem>
                                        ))}
                                    </Select>
                                </LabeledField>
                            </div>
                        </AccordionDetails>
                    </Accordion>
                </form>

                <Button
                    variant="contained"
                    onClick={handleSubmit}
                    disabled={!selectedBadgeType}
                    sx={{
                        alignSelf: "center",
                        textTransform: "none",
                        borderRadius: "9999px",
                        px: 4,
                        backgroundColor: "var(--color-primary)",
                        "&:hover": { backgroundColor: "var(--color-primary-dark)" },
                    }}
                >
                    Stage badge
                </Button>
            </div>
        </section>
    );
}
