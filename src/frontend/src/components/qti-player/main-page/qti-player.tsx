/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import type { QtiTest } from "@citolab/qti-components";
import type { ComputedContext } from "@citolab/qti-components";
import type { LevelResponse } from "../../../types/level.ts";
import { itemErrors } from "../../../types/report.ts";
import ReportForm from "../popups/report-form.tsx";
import { useState, useRef, useMemo } from "react";
import "./qti-player.css";
import QtiPlayerNavbar from "./qti-player-navbar.tsx";
import { AnimatePresence, motion, type AnimationDefinition } from "motion/react";
import QtiTestReflection from "../reflection-page/qti-test-reflection.tsx";
import QtiLeftSideBar from "./qti-left-sidebar.tsx";
import QtiAiFeedback from "../feedback-page/qti-ai-feedback.tsx";
import ComputedContextHelper from "../../../utils/qti/computed-context-helper.ts";
import QtiTestButtons from "../miscellaneous/qti-test-buttons.tsx";
import MediaQuery from "react-responsive";
import { QtiButton } from "../miscellaneous/qti-buttons.tsx";
import { useNavigate } from "react-router-dom";
import { SubmitUserLevel } from "../../../api/level.ts";
import { ConfirmExit } from "../popups/confirm-exit.tsx";
import { popupBeforeUnload } from "../../../utils/popupBeforeUnload.ts";
import { usePostHog } from "posthog-js/react";
import { CorrectAnswerViewer } from "../miscellaneous/correct-answer-viewer.tsx";
import { ConfirmFinish } from "../popups/confirm-finish.tsx";

interface QtiPlayerProps {
    level: LevelResponse;
}

/**
 * Adjusts the sizes of all `<img>` elements within the given root element or shadow root.
 *
 * - If an image's natural width exceeds its parent's width, sets the image to fill its parent
 *   by applying `display: block`, `width: 100%`, `height: 100%`, and `objectFit: cover`.
 * - Removes margins and paddings from parent `<p>` tags to ensure proper resizing.
 * - Recursively processes shadow roots found within descendant elements.
 *
 * @param root - The root HTMLElement or ShadowRoot to search for images and shadow roots.
 */
function patchImageSizes(root: HTMLElement | ShadowRoot) {
    const imgs = root.querySelectorAll("img");

    imgs.forEach((img) => {
        const parent = img.parentElement!;

        const parentWidth = parent.clientWidth;
        const naturalWidth = img.naturalWidth;

        if (naturalWidth > parentWidth) {
            img.style.display = "block";
            img.style.width = "100%";
            img.style.height = "100%";
            img.style.objectFit = "cover";
        }

        // images are often located in <p> tags
        // to make the resize work, we need to purge
        // the margins & paddings of the parent <p> tag
        const parentP = img.closest("p");
        if (parentP) {
            parentP.style.margin = "0";
            parentP.style.padding = "0";
        }
    });

    const allElements = root.querySelectorAll("*");

    allElements.forEach((el) => {
        const shadow = el.shadowRoot;
        if (shadow) {
            patchImageSizes(shadow);
        }
    });
}

export type QtiPlayerNavigation = number | "finished";

function QtiPlayer(props: QtiPlayerProps) {
    const reactNavigate = useNavigate();
    const posthog = usePostHog();

    // ref to the <qti-test> html element
    const qtiTestRef = useRef<QtiTest>(null);
    // the computed context, aka test data
    const [computedContext, setComputedContext] = useState<ComputedContext | null>(null);
    // set to true when navigating
    const [playerLoading, setPlayerLoading] = useState<boolean>(true);
    // set to true when the player is no longer visible
    const [playerDeloaded, setPlayerDeloaded] = useState<boolean>(false);
    // whether to perform a navigation to another question
    const [navigate, setNavigate] = useState<number>(-1);
    // whether the user has decided to end the test; i.e. when the end test button has been clicked
    const [testEnd, setTestEnd] = useState<boolean>(false);
    // which popup to show
    const [currentPopup, setCurrentPopup] = useState<"report" | "quit" | "finish" | "">("");
    const [showFeedback, setShowFeedback] = useState<boolean>(false);

    // helper class abstracting away some evaluation logic on the ComputedContext object emitted by the qti-player
    const contextHelper: ComputedContextHelper = useMemo(
        () => new ComputedContextHelper(computedContext),
        [computedContext]
    );

    function getActiveItemId(): number {
        const activeItem = contextHelper.activeItem;

        return props.level.itemIds[activeItem.index! - 1];
    }

    function navigateToItem(index: QtiPlayerNavigation) {
        if (index == "finished") return onTestEnd();
        setPlayerLoading(true);
        setNavigate(index);
    }

    /** Actions to take when the QTI player has finished loading */
    function handleQtiLoaded() {
        setPlayerDeloaded(false);
        setPlayerLoading(false);
        patchQtiImages();
    }

    /**
     * Patches overflowing images in the QTI player
     * by going into the shadow root of the player.
     * Attaches a `MutationObserver` to re-run this patch
     * when the player re-renders.
     */
    function patchQtiImages() {
        const root = qtiTestRef.current;
        if (!root) return;

        patchImageSizes(root);

        const observer = new MutationObserver(() => {
            patchImageSizes(root);
        });

        observer.observe(root, { childList: true, subtree: true });

        return () => observer.disconnect();
    }

    const onTestEnd = () => {
        if (!contextHelper.allItemsCompleted) setCurrentPopup("finish");
        else finishTest();
    };

    const finishTest = () => {
        if (computedContext == null) return;
        posthog.capture("level_completed", {
            level_id: props.level.levelId,
        });
        setPlayerLoading(true);
        setTestEnd(true);
        void SubmitUserLevel(props.level, computedContext);
    };

    popupBeforeUnload();

    return (
        <>
            <div
                className="
                    justify-center

                    grid
                    grid-cols-1

                    [grid-template-areas:'right''main''left']

                    lg:grid-cols-[3fr_1fr]
                    lg:[grid-template-areas:'main_right''left_right']

                    xl:grid-cols-[2fr_1fr]
                    xl:[grid-template-areas:'main_right''left_right']

                    2xl:grid-cols-[1fr_minmax(0,800px)_1fr]
                    2xl:[grid-template-areas:'left_main_right']
                "
            >
                {/* left-side */}
                <div
                    className="
                        [grid-area:left]
                        hidden
                        2xl:flex
                        flex-col
                        items-center
                        bg-gray-50
                        sticky
                        top-0
                        self-start
                        min-h-screen
                    "
                >
                    <QtiLeftSideBar
                        testEnd={testEnd}
                        onFeedback={() => setShowFeedback(true)}
                        onReturn={() => {
                            void reactNavigate("/");
                            posthog.capture("return_to_home_screen", {
                                level_id: props.level.levelId,
                            });
                        }}
                    />
                </div>
                {/* right-side QTI navigation */}
                <div
                    className="
                        [grid-area:right]
                        flex
                        flex-col-reverse
                        lg:flex-col
                        lg:items-start
                        bg-white
                        lg:sticky
                        lg:top-0
                        px-6
                        lg:px-0
                        overflow-hidden
                        items-center
                        pb-2
                        lg:pb-0
                        max-h-screen
                    "
                >
                    {computedContext && !testEnd && (
                        <QtiPlayerNavbar
                            computedContext={computedContext}
                            onNavigation={navigateToItem}
                            testEnd={testEnd}
                            testAbort={() => setCurrentPopup("quit")}
                            questionReported={() => setCurrentPopup("report")}
                        />
                    )}
                </div>
                {/* middle, the QTI player lives here */}
                <div
                    className="
                        [grid-area:main]
                        flex
                        flex-col
                        min-w-[200px]
                        min-h-screen
                        px-6
                        gap-4
                        mb-20
                        sm:mb-0
                    "
                >
                    <AnimatePresence mode="wait">
                        {computedContext && testEnd && playerDeloaded && !showFeedback && (
                            <motion.div
                                className="w-full h-full"
                                key="qti-reflection"
                                initial={{ opacity: 0 }}
                                animate={{ opacity: 1 }}
                                exit={{ opacity: 0 }}
                                transition={{ duration: 0.3 }}
                            >
                                <QtiTestReflection
                                    computedContext={computedContext!}
                                    onClose={() => void reactNavigate("/")}
                                    onFeedback={() => setShowFeedback(true)}
                                />
                            </motion.div>
                        )}
                        {showFeedback && (
                            <motion.div
                                key="qti-feedback"
                                initial={{ opacity: 0 }}
                                animate={{ opacity: 1 }}
                                exit={{ opacity: 0 }}
                                transition={{ duration: 0.3 }}
                            >
                                <QtiAiFeedback level={props.level} />
                            </motion.div>
                        )}
                    </AnimatePresence>
                    <motion.div
                        className={`
                            leading-relaxed
                            ${playerLoading || currentPopup == "report" ? "pointer-events-none" : ""}
                            ${testEnd ? (playerDeloaded ? "hidden" : "") : ""}
                        `}
                        initial={{
                            opacity: 0,
                        }}
                        animate={{
                            opacity: playerLoading ? 0 : 1,
                            transition: { duration: 0.15 },
                        }}
                        onAnimationComplete={(definition) => {
                            if (
                                (
                                    definition as AnimationDefinition & {
                                        [opacity: string]: number;
                                    }
                                ).opacity === 0
                            ) {
                                setPlayerDeloaded(true);
                            }
                            if (navigate >= 0) {
                                setNavigate(-1);
                                qtiTestRef.current?.navigateTo(
                                    "item",
                                    contextHelper.itemIdentifierByIndex(navigate)
                                );
                            }
                        }}
                    >
                        {/*
                            The QTI player's custom events are not typed properly.
                            This may be a result of the React integration.
                            This is also pretty hard to just fix.
                        */}
                        <qti-test
                            ref={qtiTestRef}
                            navigate="item"
                            onqti-navigation-loading-ended={handleQtiLoaded}
                        >
                            <test-navigation
                                data-testid="qti-test-navigation"
                                auto-score-items="true"
                                // the type of 'event' is wrong! -> explicitly casting to CustomEvent<ComputedContext>
                                onqti-computed-context-updated={(event) =>
                                    setComputedContext(
                                        (event as unknown as CustomEvent<ComputedContext>).detail
                                    )
                                }
                            >
                                <test-container testXML={props.level.assessmentXml} />
                            </test-navigation>
                        </qti-test>
                        {computedContext && (
                            <div className="hidden w-full justify-start mt-8 mb-12 md:flex">
                                <QtiTestButtons
                                    computedContext={computedContext}
                                    playerLoading={playerLoading}
                                    onNavigation={navigateToItem}
                                    onTestEnd={onTestEnd}
                                />
                            </div>
                        )}
                    </motion.div>
                    {import.meta.env.VITE_ENVIRONMENT == "development" && (
                        <CorrectAnswerViewer
                            visible={!testEnd && !playerLoading}
                            contextHelper={contextHelper}
                        />
                    )}
                </div>
            </div>
            <MediaQuery maxWidth={"39.99rem"}>
                {computedContext && !testEnd && (
                    <div className="flex items-center justify-around bottom-0 fixed w-screen gap-1 bg-white h-20 px-2 shadow shadow-black-900">
                        <QtiButton 
                            variant="red" 
                            compatibility="mobile"
                            onClick={() => { setCurrentPopup("quit") }}
                        >
                            Quit level
                        </QtiButton>
                        <QtiTestButtons
                            computedContext={computedContext}
                            playerLoading={playerLoading}
                            onNavigation={navigateToItem}
                            onTestEnd={onTestEnd}
                        />
                    </div>
                )}
            </MediaQuery>

            {/* Report Form overlay */}
            <ReportForm
                open={currentPopup === "report"}
                itemId={getActiveItemId}
                onClose={() => setCurrentPopup("")}
                reportOptions={itemErrors.slice()}
                reportDefault={itemErrors.slice()[0]}
            />
            <ConfirmExit
                open={currentPopup === "quit"}
                onQuit={() => {
                    posthog.capture("level_quit", {
                        level_id: props.level.levelId,
                    });
                    void reactNavigate("/");
                }}
                onStay={() => setCurrentPopup("")}
            />
            <ConfirmFinish
                open={currentPopup === "finish"}
                onFinish={() => {
                    setCurrentPopup("");
                    finishTest();
                }}
                onContinue={() => setCurrentPopup("")}
            />            
        </>
    );
}

export default QtiPlayer;
