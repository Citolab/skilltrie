/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { useEffect } from "react";

interface PopupWindowProps {
    /** whether to show the window */
    isOpen: boolean;
    /** callback function called when window is closed */
    onClose: () => void;
    /** window title */
    title?: string;
    children: React.ReactNode;
    /** width customization */
    size?: "small" | "mid" | "large";
}

const sizeClasses = {
    small: "max-w-sm",
    mid: "max-w-md",
    large: "max-w-3xl",
};

/**
 * component for displaying overlay content with a scrollable body, you can also add a title and close the window
 * @see PopupWindowProps - check the PopupWindowProps for parameters specifics
 */
function PopupWindow(props: PopupWindowProps) {
    // stops scrolling the background
    useEffect(() => {
        if (props.isOpen) {
            document.body.style.overflow = "hidden";
        } else {
            document.body.style.overflow = "";
        }
    }, [props, props.isOpen]);

    // make esc work to exit
    useEffect(() => {
        function handleEsc(e: KeyboardEvent) {
            if (e.key === "Escape") props.onClose();
        }
        if (props.isOpen) document.addEventListener("keydown", handleEsc);
        return () => document.removeEventListener("keydown", handleEsc);
    }, [props, props.isOpen, props.onClose]);

    // don't render if isOpen false
    if (!props.isOpen) return null;

    return (
        <div className="fixed inset-0 z-2 flex items-center justify-center bg-black/80">
            {/* container with dynamic size */}
            <div
                className={`relative bg-gray-200 dark:bg-gray-800 rounded-lg shadow-lg w-full ${
                    props.size ? sizeClasses[props.size] : sizeClasses["small"]
                }`}
                role="dialog"
                aria-modal="true"
            >
                {/* header with title and exit button */}
                <div className="flex items-center justify-between px-4 py-4 bg-gray-600 dark:bg-gray-700 border-b border-gray-600">
                    {props.title && (
                        <h2 className="capitalize select-none text-xl font-semibold text-white">
                            {props.title}
                        </h2>
                    )}
                    <button
                        onClick={props.onClose}
                        className="text-gray-300 hover:text-white cursor-pointer p-1"
                        aria-label="Close"
                    >
                        <svg width="24" height="24" viewBox="0 0 24 24">
                            <line
                                x1="4"
                                y1="4"
                                x2="20"
                                y2="20"
                                stroke="currentColor"
                                strokeWidth="2"
                            />
                            <line
                                x1="20"
                                y1="4"
                                x2="4"
                                y2="20"
                                stroke="currentColor"
                                strokeWidth="2"
                            />
                        </svg>
                    </button>
                </div>

                {/* scrollable content */}
                <div className="max-h-[80vh] overflow-y-auto p-6">{props.children}</div>
            </div>
        </div>
    );
}

export default PopupWindow;
