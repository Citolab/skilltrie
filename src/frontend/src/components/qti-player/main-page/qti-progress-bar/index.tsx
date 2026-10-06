/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { motion } from "motion/react";
import QtiProgressBarShimmer from "./qti-progress-bar-shimmer";

type QtiProgressBarProps = {
    numCompletedItems: number;
    numTotalItems?: number;
};

function QtiProgressBar({
    numCompletedItems,
    numTotalItems = 10
}: QtiProgressBarProps) {
    const testEnd = numCompletedItems === numTotalItems;

    return (
        <div className="flex items-center w-full">
            <div className="w-16 text-center font-bold shrink-0">
                Start
            </div>

            <div className="
                flex w-[600px] 
                h-6 
                rounded-lg
                overflow-hidden
                border border-[var(--color-borderDisabled)] 
                bg-[var(--color-background-light)]
                relative"
            >
                {testEnd && <QtiProgressBarShimmer amount={2} duration={4}/>}

                {Array.from({ length: numTotalItems }, (_, index) => {
                    const isAnswered = index < numCompletedItems;

                    return (
                        <div
                            key={index}
                            className="flex-1 h-full relative overflow-hidden"
                        >
                            <motion.div
                                className="
                                    absolute 
                                    inset-0 
                                    bg-[var(--color-primary)]"
                                initial={{ 
                                    scaleX: 0,
                                 }}
                                animate={{ 
                                    scaleX: isAnswered ? 1 : 0,
                                 }}
                                transition={{
                                    scaleX: {
                                        duration: 0.25,
                                        ease: "easeOut",
                                    },
                                }}
                                style={{ originX: 0 }}
                            />
                        </div>
                    );
                })}
            </div>

            <div className={`
                w-16 
                text-center 
                font-bold
                shrink-0
                `}
            >
                End
            </div>
        </div>
    );
}

export default QtiProgressBar;