/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { type ComputedContext } from "@citolab/qti-components";
import { motion, stagger, type Variants } from "motion/react";
import ComputedContextHelper from "../../../utils/qti/computed-context-helper";
import QTIReflectionItem from "./qti-reflection-item";
import Accordion from "@mui/material/Accordion";
import AccordionSummary from "@mui/material/AccordionSummary";
import AccordionDetails from "@mui/material/AccordionDetails";
import { ChevronDownIcon } from "@heroicons/react/24/solid";

interface QtiResultsTabulationProps {
    computedContext: ComputedContext;
    onAnimationComplete?: () => void;
}

function QtiResultsTabulation(props: QtiResultsTabulationProps) {
    const contextHelper = new ComputedContextHelper(props.computedContext);

    const list = {
        visible: {
            opacity: 1,
            transition: {
                when: "beforeChildren",
                delayChildren: stagger(
                    Math.max(0.05, 1 / (contextHelper.activeSectionItems.length * 2))
                ),
            },
        },
        hidden: {
            opacity: 0,
            transition: {
                when: "afterChildren",
            },
        },
    } as const satisfies Variants;

    const items = {
        visible: { opacity: 1, y: 0 },
        hidden: { opacity: 0, y: 20 },
    } as const satisfies Variants;

    return (
        <motion.div
            variants={list}
            initial="hidden"
            animate="visible"
            className="flex flex-col items-center sm:items-start gap-3 pb-4"
            onAnimationComplete={(definition) => {
                if (definition === "visible") props.onAnimationComplete?.();
            }}
        >
            {contextHelper.activeSectionItems.map((item) => {
                const answered = item.completionStatus === "completed";
                const correct = item.score != null && item.score === item.maxScore;

                return (
                    <motion.div key={item.index} variants={items} className="w-full">
                        <Accordion
                            disableGutters
                            sx={{
                                borderRadius: "1rem !important",
                                border: "3px solid #e5e7eb",
                                boxShadow: "none",
                                overflow: "hidden",
                            }}
                        >
                            <AccordionSummary
                                expandIcon={<ChevronDownIcon className="h-4 w-4 text-gray-500" />}
                                sx={{ px: 2, py: 1.5 }}
                            >
                                <div className="grid grid-cols-[1fr_auto] items-center w-full pr-2">
                                    <span className="text-color-primary font-nunito">
                                        Question {item.index}
                                    </span>
                                    <div className="flex items-center gap-2 text-gray-500 text-color-primary font-nunito">
                                        <span
                                            className={`inline-block h-3 w-3 rounded-full ${
                                                answered
                                                    ? correct
                                                        ? "bg-emerald-400"
                                                        : "bg-rose-400"
                                                    : "bg-gray-300"
                                            }`}
                                        />
                                        {answered
                                            ? correct
                                                ? "Correct"
                                                : "Incorrect"
                                            : "Not Answered"}
                                    </div>
                                </div>
                            </AccordionSummary>
                            <AccordionDetails sx={{ px: 2, pb: 2 }}>
                                <QTIReflectionItem item={item} />
                            </AccordionDetails>
                        </Accordion>
                    </motion.div>
                );
            })}
        </motion.div>
    );
}

export default QtiResultsTabulation;
