/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { XMarkIcon } from "@heroicons/react/24/outline";
import Box from "@mui/material/Box";
import Fade from "@mui/material/Fade";
import Modal from "@mui/material/Modal";

type PopUpButtonVariant = "green" | "red" | "cyan" | "yellow" | "gray" | "primary";

const headerClasses: Record<PopUpButtonVariant, string> = {
    green: `
        bg-green-500
        `,
    red: `
        bg-red-500
        `,
    cyan: `
        bg-cyan-500
        `,
    yellow: `
        bg-yellow-500
        `,
    gray: `
        bg-gray-500
        `,
    primary: `
        bg-primary
        text-white
        `,
};

const variantClasses: Record<PopUpButtonVariant, string> = {
    green: `
        border-qti-button-green-border
        text-qti-button-green-text
        hover:bg-qti-button-green-hover
    `,
    red: `
        border-qti-button-red-border
        text-qti-button-red-text
        hover:bg-qti-button-red-hover
    `,
    cyan: `
        border-qti-button-cyan-border
        text-qti-button-cyan-text
        hover:bg-qti-button-cyan-hover
    `,
    yellow: `
        border-qti-button-yellow-border
        text-qti-button-yellow-text
        hover:bg-qti-button-yellow-hover
    `,
    gray: `
        border-gray-300
        text-gray-500
        hover:bg-gray-100
    `,
    primary: `
        border-primary-dark
        text-primary-dark
        hover:bg-primary-ghost
    `,
};

interface IPopupLayoutProps {
    open: boolean;
    title: string;
    headerClassName: PopUpButtonVariant;
    onClose: () => void; // 'X' and the secondary button by default
    primaryAction?: { label: string; onClick: () => void }; //This className default to the color palette of the header.
    secondaryAction?: { label: string; onClick?: () => void; className?: string };
    dynamicSizing?: boolean;
    children: React.ReactNode;
}

export const PopupLayout = ({
    open,
    title,
    headerClassName,
    onClose,
    primaryAction,
    secondaryAction,
    dynamicSizing,
    children,
}: IPopupLayoutProps) => {
    const resolvedPrimary = {
        label: primaryAction?.label ?? "Submit",
        onClick: primaryAction?.onClick ?? (() => {}),
    };

    const resolvedSecondary = {
        label: secondaryAction?.label ?? "Cancel",
        onClick: secondaryAction?.onClick ?? onClose,
        className: secondaryAction?.className ?? "border-gray-300 text-gray-500 hover:bg-gray-100",
    };
    return (
        <Modal open={open} onClose={onClose} closeAfterTransition>
            <Fade in={open}>
                <Box
                    className={`absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 max-w-9/10 outline-none ${dynamicSizing ? "" : "w-[90%] md:w-[50%] lg:w-[30%]"}`}
                >
                    <div className={`flex rounded-t-[10px] ${headerClasses[headerClassName]}`}>
                        <div className="p-2.5">{title}</div>
                        <button
                            className="ml-auto p-2.5 hover:bg-gray-400 rounded-tr-[10px] cursor-pointer"
                            onClick={onClose}
                            aria-label="Close"
                        >
                            <XMarkIcon className="h-5 w-5" />
                        </button>
                    </div>

                    <div className="flex flex-col p-2.5 bg-gray-200 rounded-b-[10px] ">
                        {children}

                        <div className="flex flex-row justify-center gap-x-3 mt-4">
                            {primaryAction && (
                                <button
                                    className={`p-2.5 rounded-[10px] ring-1 cursor-pointer mx-auto ${variantClasses[headerClassName] ?? "bg-gray-300 border-gray-300 hover:bg-gray-100 text-gray-500"}`}
                                    onClick={resolvedPrimary.onClick}
                                >
                                    {resolvedPrimary.label}
                                </button>
                            )}
                            {secondaryAction && (
                                <button
                                    className={`p-2.5 rounded-[10px] ring-1 cursor-pointer mx-auto ${resolvedSecondary.className}`}
                                    onClick={resolvedSecondary.onClick ?? onClose}
                                >
                                    {resolvedSecondary.label}
                                </button>
                            )}
                        </div>
                    </div>
                </Box>
            </Fade>
        </Modal>
    );
};
