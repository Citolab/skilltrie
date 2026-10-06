/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import type { ComputedContext } from "@citolab/qti-components";
import { AnimatePresence, motion, stagger } from "motion/react";
import { useCallback, useEffect, useRef } from "react";
import { QtiButton, QtiGradientButton } from "../miscellaneous/qti-buttons";
import ComputedContextHelper from "../../../utils/qti/computed-context-helper";
import type { QtiPlayerNavigation } from "./qti-player.tsx";
import { ExclamationTriangleIcon } from "@heroicons/react/24/solid";
import QtiProgressBar from  "./qti-progress-bar/index.tsx";

interface QtiPlayerNavbarProps {
    computedContext: ComputedContext;
    testEnd: boolean;
    testAbort?: () => void;
    questionReported?: () => void;
    onNavigation: (index: QtiPlayerNavigation) => void;
}

function QtiPlayerNavbar(props: QtiPlayerNavbarProps) {
    const contextHelper = new ComputedContextHelper(props.computedContext);

    const items = contextHelper.activeSectionItems;

    const getActiveIndex = useCallback(() => {
        return items.findIndex((i) => i.active);
    }, [items]);

    const list = {
        visible: {
            opacity: 1,
            transition: {
                when: "beforeChildren",
                delayChildren: stagger(Math.max(0.05, 1 / (items.length * 2))), // delay per child
            },
        },
        hidden: {
            opacity: 0,
            transition: {
                when: "afterChildren",
            },
        },
    };

    const item = {
        visible: { opacity: 1, x: 0 },
        hidden: { opacity: 0, x: 10 },
    };

    const itemRefs = useRef<(HTMLDivElement | null)[]>([]);

    useEffect(() => {
        const element = itemRefs.current[getActiveIndex()];

        if (!element) return;

        element.scrollIntoView({
            behavior: "smooth",
            inline: "start",
            block: "start",
        });
    }, [getActiveIndex]);

    const itemNumberColor = (completionStatus: string) => {
        return completionStatus === "completed"
            ? { textColor: "text-blue-500", bgColor: "bg-blue-500" }
            : completionStatus === "unknown"
              ? { textColor: "text-warning", bgColor: "bg-warning" }
              : { textColor: "text-gray-400", bgColor: "bg-gray-400" }; // completionStatus: "not_attempted"
    };

    let numCompletedItems = items.filter(i => i.completionStatus == "completed").length;

    return (
        <>
            <h1 className="py-4 text-2xl text-center lg:inline-block hidden">Level Progress</h1>
            <QtiProgressBar numCompletedItems={numCompletedItems} numTotalItems={items.length}/>
            <h1 className="py-4 text-2xl text-center lg:inline-block hidden">Navigation</h1>
            <motion.div
                className="flex lg:flex-col overflow-x-auto w-full min-w-0 sm:justify-start"
                variants={list}
                initial="hidden"
                animate="visible"
            >
                {items.map((i, index) => {
                    const { textColor, bgColor } = itemNumberColor(i.completionStatus!);
                    return (
                        <motion.div
                            key={i.index}
                            variants={item}
                            ref={(element) => {
                                itemRefs.current[index] = element;
                            }}
                            layoutId=""
                            onClick={() => props.onNavigation(i.index != null ? i.index - 1 : -1)}
                            className={`
                            flex
                            shrink-0
                            relative
                            content-center
                            items-center
                            lg:justify-start
                            justify-center
                            h-12 w-12
                            lg:pl-6
                            font-bold
                            ${textColor}
                            box-border
                            transition-colors
                            cursor-pointer
                            ${getActiveIndex() === index && !props.testEnd ? "border-l-black cursor-pointer" : "border-l-transparent"}
                        `}
                        >
                            {index === getActiveIndex() && !props.testEnd ? (
                                <motion.div
                                    className={`${bgColor} absolute left-0 bottom-0 lg:top-0 h-0.5 w-full lg:w-0.5 lg:h-full`}
                                    layoutId="activeLine"
                                    id="activeLine"
                                    transition={{ duration: 0.2 }}
                                />
                            ) : null}
                            <div className="flex flex-col sm:flex-row items-center">
                                <div className="w-4"> {i.index} </div>
                                <div className="w-4">
                                    {i.completionStatus === "unknown" &&
                                        getActiveIndex() !== index && (
                                            <div className={`h-fit ml-0 lg:ml-3 mt-1 ${textColor}`}>
                                                <ExclamationTriangleIcon className="size-4 sm:size-8" />
                                            </div>
                                        )}
                                </div>
                            </div>
                        </motion.div>
                    );
                })}
                <motion.div
                    variants={item}
                    layoutId=""
                    className={`
                        flex
                        shrink-0
                        relative
                        items-center
                        lg:justify-start
                        justify-center
                        h-12 w-12
                        lg:pl-6
                        font-bold
                        text-[rgb(50,50,50)]
                        box-border
                        transition-colors
                        ${props.testEnd ? "border-l-black cursor-pointer" : "border-l-transparent"}
                    `}
                >
                    {props.testEnd && (
                        <motion.div
                            className="absolute left-0 bottom-0 lg:top-0 bg-black h-0.5 w-full lg:w-0.5 lg:h-full"
                            layoutId="activeLine"
                            id="activeLine"
                            transition={{ duration: 0.2 }}
                        />
                    )}
                    End
                </motion.div>
            </motion.div>
            <div className="w-full flex items-center lg:justify-start justify-between lg:mt-6">
                <h1 className="py-4 text-3xl text-center inline-block lg:hidden">Level Progress</h1>
                <AnimatePresence>
                    {!props.testEnd && (
                        <motion.div
                            className="flex lg:flex-col gap-6"
                            initial={{
                                opacity: 1,
                            }}
                            exit={{
                                opacity: 0,
                            }}
                            transition={{
                                duration: 0.3,
                            }}
                        >
                            <QtiButton variant="red" className="min-w-40" onClick={props.questionReported}>
                                Report question
                            </QtiButton>
                            <div className="hidden sm:flex">
                                <QtiButton variant="red" onClick={props.testAbort} data-testid="nav-stop-test">
                                    Quit level
                                </QtiButton>
                            </div>
                            <QtiGradientButton
                                className="text-md px-2 py-2"
                                onClick={() => props.onNavigation("finished")}
                            >
                                Finish Level
                            </QtiGradientButton>
                        </motion.div>
                    )}
                </AnimatePresence>
            </div>
        </>
    );
}

export default QtiPlayerNavbar;
