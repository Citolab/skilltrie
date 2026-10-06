/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { type ComputedContext } from "@citolab/qti-components";
import { useRive, Layout, Fit, Alignment, EventType } from "@rive-app/react-canvas";
import { motion, type Variants } from "motion/react";
import { useEffect, useState, type ReactNode } from "react";
import QtiResultsTabulation from "./qti-results-tabulation";
import { QtiButton } from "../miscellaneous/qti-buttons";
import MediaQuery from "react-responsive";
import QtiPopupTitle from "./qti-popup-title";
import ComputedContextHelper from "../../../utils/qti/computed-context-helper";

interface QtiTestReflectionProps {
    computedContext: ComputedContext;
    /** called when ending the test */
    onClose: () => void;
    /** called when feedback is requested */
    onFeedback: () => void;
}

const sirNoseAnimationStates = {
    initial: {
        x: 500,
        opacity: 0.5,
    },
    onEntry: {
        x: 0,
        opacity: 1,
    },
    fadeOut: {
        opacity: 0,
        x: 0,
    },
} satisfies Variants;

type LevelCorrectness = "success" | "intermediate" | "fail" | "undetermined";

const levelCorrectnessMessages: Record<LevelCorrectness, ReactNode> = {
    // Message saying: Test passed \n Well done!
    success: <div className="text-center space-y-3">
        <div className="lg:text-8xl md:text-7xl text-6xl">
            Test <span className="text-green-500">passed</span>
        </div>
        <div className="lg:text-7xl md:text-6xl text-5xl">
            Well done!
        </div>
    </div>,
    // Message saying: Almost passed \n Nice try!
    intermediate: <div className="text-center space-y-3">
        <div className="lg:text-8xl md:text-7xl text-6xl">
            <span className="text-yellow-500">Almost</span> passed
        </div>
        <div className="lg:text-7xl md:text-6xl text-5xl">
            Nice try!
        </div>
    </div>,
    // Message saying: Test failed \n Better luck next time!
    fail: <div className="text-center space-y-3">
        <div className="lg:text-8xl md:text-7xl text-6xl">
            Test <span className="text-red-500">failed</span>
        </div>
        <div className="lg:text-7xl md:text-6xl text-5xl">
            Better luck next time!
        </div>
    </div>,
    undetermined: <div>...</div>,
}

function determineLevelCorrectness(computedContext: ComputedContext) : LevelCorrectness {
    const contextHelper = new ComputedContextHelper(computedContext);
    const successThreshold = 0.8;
    const intermediateThreshhold = 0.5;
    let correct: number = 0;
    let total: number = 0;

    // Calculate the total amount of items and the amount of correctly answered items
    contextHelper.activeSectionItems.map((item) => {
        total++;
        
        if (item.score && item.score === item.maxScore)
            correct++;
    });

    // Determine the correctness of the level based on the percentage of correct items
    if (total === 0)
        return "undetermined";
    else
    {
        const ratio = correct / total;

        if (ratio >= successThreshold)
            return "success";
        else if (ratio >= intermediateThreshhold)
            return "intermediate";
        else
            return "fail";
    }
}

function QtiTestReflection(props: QtiTestReflectionProps) {
    // eslint-disable-next-line @typescript-eslint/naming-convention
    const { RiveComponent, rive } = useRive({
        src: window.location.origin + "/rive/meneer_neus2.riv",
        artboard: "thumbs up nose",
        layout: new Layout({
            fit: Fit.Contain,
            alignment: Alignment.Center,
        }),
    });

    const [sirNoseState, setSirNoseState] =
        useState<keyof typeof sirNoseAnimationStates>("onEntry");

    const [showSirNose, setShowSirNose] = useState<boolean>(true);
    const [showResults, setShowResults] = useState<boolean>(false);

    useEffect(() => {
        rive?.on(EventType.Stop, () => {
            setTimeout(() => {
                setSirNoseState("fadeOut");
            }, 300);
        });
    }, [rive]);

    const [levelCorrectness, setLevelCorrectness] = useState<LevelCorrectness>("undetermined");

    useEffect(() => {
        // On page startup: calculate and store the correctness of the level to later determine what message to display
        setLevelCorrectness(determineLevelCorrectness(props.computedContext));
        // when something can be displayed (i.e., when this function runs), scroll to top of page to properly display text
        window.scrollTo(0, 0)
    }, [props.computedContext])

    return (
        <div className="flex flex-col h-full">
            <QtiPopupTitle
                textFrom={levelCorrectnessMessages[levelCorrectness]}
                textTo="Your Results"
                onAnimationComplete={() => {
                    setShowResults(true);
                }}
            />

            {showResults && <QtiResultsTabulation computedContext={props.computedContext} />}

            <MediaQuery maxWidth={"95.99rem"}>
                {showResults && (
                    <motion.div
                        animate={{ opacity: 1 }}
                        initial={{ opacity: 0 }}
                        className="my-8 w-full flex flex-col sm:flex-row items-center sm:items-start gap-2 *:min-w-45"
                    >
                        <QtiButton variant="cyan" onClick={props.onFeedback}>Feedback</QtiButton>
                        <QtiButton variant="yellow" onClick={props.onClose}>Exit</QtiButton>
                    </motion.div>
                )}
            </MediaQuery>

            {(showSirNose && levelCorrectness === "success") && <motion.div
                className="flex-1"
                variants={sirNoseAnimationStates}
                initial="initial"
                animate={sirNoseState}
                transition={{
                    duration: 0.3,
                }}
                onAnimationComplete={(definition) => {
                    if (definition === "onEntry") {
                        rive?.play();
                    }
                    if (definition === "fadeOut") setShowSirNose(false);
                }}
            >
                <RiveComponent className="h-full w-full"/>
            </motion.div>}
        </div>
    );
}

export default QtiTestReflection;
