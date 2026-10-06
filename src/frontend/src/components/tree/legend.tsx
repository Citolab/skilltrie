/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */
import Accordion from "@mui/material/Accordion";
import AccordionSummary from "@mui/material/AccordionSummary";
import AccordionDetails from "@mui/material/AccordionDetails";
import { InformationCircleIcon } from "@heroicons/react/24/outline";
import { startHomeTour } from "../../components/tours/home-tour";
import { Button } from "@mui/material";
import { Panel } from "@xyflow/react";
import { useState } from "react";
import { useHomeTourContext } from "../../contexts/home-tour-context";

const legendItems = [
    { type: "mastered", label: "Mastered", tailwind: "border-4 border-emerald-400 bg-slate-100" },
    { type: "unlocked", label: "Unlocked", tailwind: "border-4 border-primary-light bg-slate-100" },
    { type: "locked", label: "Locked", tailwind: "bg-gray-300" },
    {
        type: "recommended",
        label: "Recommended",
        tailwind: "border-4 border-blue-200 bg-blue-50 ring-4 ring-primary-dark",
    },
];

export function Legend() {
    const [expanded, setExpanded] = useState(false);
    const { setTourActive } = useHomeTourContext();

    return (
        <Panel position="top-left">
            <Accordion
                expanded={expanded}
                onChange={(_, isExpanded) => setExpanded(isExpanded)}
                disableGutters
                elevation={0}
                sx={{
                    width: "fit-content",
                    minWidth: "unset",
                    borderRadius: 2,
                    overflow: "visible",
                    backgroundColor: "var(--color-white)",
                    "&:before": { display: "none" },
                }}
            >
                <AccordionSummary
                    sx={{
                        minHeight: "unset",
                        padding: "6px",
                        borderRadius: 2,
                        boxShadow: 2,
                        transition: "box-shadow 0.1s ease, transform 0.1s ease",
                        "& .MuiAccordionSummary-content": {
                            margin: 0,
                            alignItems: "center",
                        },
                        "&:hover": {
                            backgroundColor: "var(--color-grey-100)",
                            cursor: "pointer",
                        },
                        "&:active": {
                            boxShadow: 0,
                            transform: "translateY(2px)",
                        },
                    }}
                >
                    <InformationCircleIcon className="w-7 h-7" />
                </AccordionSummary>
                <AccordionDetails
                    sx={{
                        position: "absolute",
                        backgroundColor: "var(--color-white)",
                        borderRadius: 2,
                        marginTop: "4px",
                        width: "200px",
                        boxShadow: 3,
                    }}
                >
                    <div className="flex flex-col gap-2 text-xs font-medium">
                        <p className="text-sm font-semibold mt-2 mb-0">Topic info:</p>
                        {legendItems.map(({ type, label, tailwind }) => (
                            <div key={type} className="flex items-center gap-2">
                                <span className={`w-4 h-4 rounded shrink-0 ${tailwind}`} />
                                <span>{label}</span>
                            </div>
                        ))}
                        <p className="text-sm font-semibold mt-2 mb-0">Mastery:</p>
                        <p className="text-sm -mt-2 mb-2">
                            To master a topic, you must get a score of at least 80% on a test!
                        </p>
                        <Button
                            variant="contained"
                            size="small"
                            onClick={() => {
                                setExpanded(false);
                                startHomeTour(setTourActive);
                            }}
                        >
                            Start tutorial
                        </Button>
                    </div>
                </AccordionDetails>
            </Accordion>
        </Panel>
    );
}
