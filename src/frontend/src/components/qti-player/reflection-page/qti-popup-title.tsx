/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { AnimatePresence, motion } from "motion/react";
import { useState, type ReactNode } from "react";

interface QtiPopupTitleProps {
    textFrom: ReactNode;
    textTo: ReactNode;
    onAnimationComplete: () => void;
}

function QtiPopupTitle(props: QtiPopupTitleProps) {
    const [titleChange, setTitleChange] = useState<boolean>(false);
    const [titlePresent, setTitlePresent] = useState<boolean>(true);

    return (
        <div className="p-3 w-full flex flex-col justify-center">
            <AnimatePresence>
                {!titleChange && (
                    <motion.h1
                        className="
                            flex
                            w-fit
                            self-center
                            font-bold
                            bg-clip-text text-transparent
                        "
                        exit={{
                            opacity: 0,
                            scale: 0,
                        }}
                        initial={{
                            opacity: 0,
                            y: 50,
                            backgroundImage:
                                "linear-gradient(360deg,rgba(0,177,170,1) 0%,rgba(132,191,147,1) 50%,rgba(246,201,0,1) 100%)",
                        }}
                        animate={{
                            opacity: 1,
                            y: 0,
                            backgroundImage:
                                "linear-gradient(45deg,rgba(0,177,170,1) 0%,rgba(132,191,147,1) 50%,rgba(246,201,0,1) 100%)",
                        }}
                        transition={{
                            duration: 0.5,
                        }}
                        onAnimationComplete={(definition) => {
                            // @ts-expect-error presence of scale property is irrelevant to if statement
                            if (definition.scale === 0) setTitlePresent(false);
                            setTimeout(() => setTitleChange(true), 1000);
                        }}
                    >
                        {props.textFrom}
                    </motion.h1>
                )}
            </AnimatePresence>
            {!titlePresent && (
                <motion.h1
                    className="
                        flex
                        w-fit
                        text-5xl
                        self-center
                        sm:self-start
                    "
                    initial={{
                        opacity: 0,
                    }}
                    animate={{
                        opacity: 1,
                    }}
                    onAnimationComplete={props.onAnimationComplete}
                >
                    {props.textTo}
                </motion.h1>
            )}
        </div>
    );
}

export default QtiPopupTitle;
