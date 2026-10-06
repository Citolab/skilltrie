/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import React from "react";
import { type CSSProperties } from "react";

interface GeneralButtonProps {
    id?: string;
    children: React.ReactNode;
    onClick?: () => void;
    positionStyling?: PositionStyles;
    type?: "button" | "submit" | "reset";
    variant?: "primary" | "secondary" | "danger" | "ghost" | "outline";
    size?: "xsmall" | "small" | "medium" | "large";
    disabled?: boolean;
    fullWidth?: boolean;
}

type PositionStyles = Pick<
    CSSProperties,
    "position" | "top" | "bottom" | "left" | "right" | "zIndex" | "inset" | "transform"
>;

export default function GeneralButton({
    id,
    children,
    onClick,
    positionStyling,
    type = "button",
    variant = "primary",
    size = "medium",
    disabled = false,
    fullWidth = false,
}: GeneralButtonProps) {
    // Define button variant tailwind css here
    const variants = {
        primary:
            "bg-primary text-text-light rounded-full font-button font-semibold hover:bg-primary-dark ",
        secondary:
            "bg-secondary text-text-light rounded-full font-button font-semibold hover:bg-secondary-dark ",
        danger: "bg-warning text-text-light rounded-full font-button font-semibold hover:bg-warning-dark ",
        ghost: "text-slate-600 rounded-full font-button font-semibold hover:bg-slate-100 ",
        outline:
            "bg-transparent text-primary-500 border border-black rounded-full font-button font-semibold hover:bg-slate-100",
    };

    // Sizes for buttons.
    const sizes = {
        xsmall: "px-2 py-1 text-xs",
        small: "px-3 py-1.5 text-sm",
        medium: "px-6 py-3 text-base",
        large: "px-8 py-4 text-xl",
    };

    return (
        <button
            id={id}
            type={type}
            onClick={onClick}
            disabled={disabled}
            className={`
                cursor-pointer hover:shadow-xl
                active:translate-y-0.5 transition-all duration-50
                ${variants[variant]}
                ${sizes[size]}
                ${fullWidth ? "w-full" : ""}
            `}
            style={positionStyling}
        >
            {children}
        </button>
    );
}

// TODO: transition-all duration-150 active:scale-105" used later for nodes
