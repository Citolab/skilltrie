/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { useRive, Layout, Fit, Alignment, EventType } from "@rive-app/react-canvas";
import { AnimatePresence, easeOut, motion, type Variants } from "motion/react";
import { useEffect, useState } from "react";
import { QtiButton } from "../miscellaneous/qti-buttons";

interface QtiLeftSideBarProps {
    testEnd: boolean;
    onFeedback?: () => void;
    onReturn?: () => void;
}

function QtiLeftSideBar(props: QtiLeftSideBarProps) {
    // eslint-disable-next-line @typescript-eslint/naming-convention
    const { RiveComponent, rive } = useRive({
        src: window.location.origin + "/rive/meneer_neus5.riv",
        artboard: "meneer neus",
        stateMachines: "wave",
        autoplay: true,
        layout: new Layout({
            fit: Fit.Contain,
            alignment: Alignment.Center,
        }),
    });

    const [showIdle, setShowIdle] = useState<boolean>(false);

    const [showButtons, setShowButtons] = useState<boolean>(false);

    const [showBottom, setShowBottom] = useState<boolean>(true);

    useEffect(() => {
        if (showIdle) {
            rive?.reset({
                artboard: "meneer neus",
                stateMachines: undefined,
                autoplay: true,
            });
            rive?.play();
        }
    }, [showIdle]);

    const sirNoseAnimationStates = {
        initial: {
            x: -700,
        },
        onAppear: {
            x: 0,
            transition: {
                duration: 1,
                ease: [0.53, 1.86, 0.8, 0.76],
            },
        },
        onDisappear: {
            x: 0,
            scale: 0,
            transition: {
                duration: 0.4,
                ease: easeOut,
            },
        },
    } as const satisfies Variants;

    rive?.on(EventType.Stop, () => {
        if (!showIdle) setShowIdle(true);
    });

    return (
        <>
            <div className="h-screen min-w-100 flex flex-col items-center justify-center overflow-clip">
                {!showButtons && (
                    <motion.div
                        variants={sirNoseAnimationStates}
                        initial="initial"
                        animate={!props.testEnd ? "onAppear" : "onDisappear"}
                        className="w-full aspect-square"
                        onAnimationComplete={(definition) => {
                            if (definition === "onDisappear") {
                                setShowButtons(true);
                            }
                        }}
                    >
                        <RiveComponent />
                    </motion.div>
                )}

                {showButtons && (
                    <motion.div
                        className="h-full w-full flex flex-col justify-evenly items-center"
                        initial={{
                            x: "-100%",
                        }}
                        animate={{
                            x: 0,
                        }}
                        transition={{
                            duration: 0.5,
                            ease: "easeOut",
                        }}
                    >
                        <motion.div layout>
                            <QtiButton variant="yellow" className="min-w-45" onClick={props.onReturn}>
                                Exit
                            </QtiButton>
                        </motion.div>

                        <AnimatePresence mode="popLayout">
                            {showBottom && (
                                <motion.div
                                    layout
                                    initial={{ opacity: 0 }}
                                    animate={{ opacity: 1 }}
                                    exit={{ opacity: 0 }}
                                    transition={{ duration: 0.3 }}
                                    onClick={() => setShowBottom(false)}
                                >
                                    <QtiButton variant="cyan" className="min-w-45" onClick={props.onFeedback}>
                                        Feedback
                                    </QtiButton>
                                </motion.div>
                            )}
                        </AnimatePresence>
                    </motion.div>
                )}
            </div>
        </>
    );
}

export default QtiLeftSideBar;
