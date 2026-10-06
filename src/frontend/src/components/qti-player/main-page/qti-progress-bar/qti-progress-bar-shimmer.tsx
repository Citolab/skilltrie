/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { motion } from "motion/react";

type QtiProgressBarShimmerProps = {
    // Amount of simultaneous shimmer waves
    amount: number;
    // The time between a shimmer wave cycle
    duration: number;
};

function QtiProgressBarShimmer({
    amount = 2,
    duration = 4,
}: QtiProgressBarShimmerProps) {
    return (
        <>
            {Array.from({ length: amount }, (_, index) => (
                <motion.div
                    key={index}
                    className="
                        absolute
                        inset-y-0
                        w-64
                        bg-gradient-to-r
                        from-transparent
                        via-white/40
                        to-transparent
                        z-20
                        pointer-events-none
                    "
                    animate={{
                        x: ["-100%", "150%"],
                    }}
                    transition={{
                        duration,
                        repeat: Infinity,
                        ease: "linear",
                        delay: (duration / amount) * index,
                    }}
                />
            ))}
        </>
    );
}

export default QtiProgressBarShimmer;