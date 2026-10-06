/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import type React from "react";

type QtiButtonVariant =
    | "primary"
    | "red"
    | "green"
    | "yellow"
    | "cyan"
    | "gradient" // Should only be used by a QtiGradientButton

const variantClasses: Record<QtiButtonVariant, string> = {
    primary: `
        border-qti-button-primary-border
        text-qti-button-primary-text
        hover:bg-qti-button-primary-hover
    `,
    red: `
        border-qti-button-report-border
        text-qti-button-report-text
        hover:bg-qti-button-report-hover
    `,
    green: `
        border-qti-button-green-border
        text-qti-button-green-text
        hover:bg-qti-button-green-hover
    `,
    yellow: `
        border-qti-button-yellow-border
        text-qti-button-yellow-text
        hover:bg-qti-button-yellow-hover
    `,
    cyan: `
        border-qti-button-cyan-border
        text-qti-button-cyan-text
        hover:bg-qti-button-cyan-hover
    `,
    gradient: `
        border-[#00b1aa]
        bg-white
        hover:bg-gray-50
    `,
};

type QtiButtonCompatibility = 
    | "general"
    | "mobile"

const compatibilityClasses: Record<QtiButtonCompatibility, string> = {
    general: `
        px-4 py-2
        rounded-lg
        max-w-fit
        flex items-center justify-center
        transition-colors duration-200
    `,
    mobile: `
        px-5 py-3
        rounded-md
        text-lg
        min-w-35
    `,
};

type QtiButtonProps = {
    variant: QtiButtonVariant;
    compatibility?: QtiButtonCompatibility;
    onClick?: () => void;
    disabled?: boolean;
    className?: string;
    children: React.ReactNode;
};

export function QtiButton({
    variant,
    compatibility = "general",
    className,
    children,
    ...props
}: QtiButtonProps) {
    return (
        <button
            className={`
                cursor-pointer
                border-2
                font-medium
                ${variantClasses[variant]}
                ${compatibilityClasses[compatibility]}
                ${className ?? ""}
            `}
            {...props}
        >
        {children}
        </button>
    );
} 

type QtiGradientButtonProps = 
    Omit<QtiButtonProps, "variant"> & {
        textClassName?: string;
    };

export function QtiGradientButton ({
    children,
    textClassName = "",
    ...props
}: QtiGradientButtonProps) {
   return (
        <QtiButton 
            variant="gradient"
            {...props}
        >
            <span className={`
                bg-[linear-gradient(45deg,rgba(0,177,170,1)_0%,rgba(132,191,147,1)_50%,rgba(246,201,0,1)_100%)]
                bg-clip-text
                text-transparent
                ${textClassName}
            `}
            >
                {children}
            </span>
        </QtiButton>);
}
