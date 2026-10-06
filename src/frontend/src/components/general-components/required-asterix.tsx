/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

interface RequiredAsterixProps {
    size?: "xs" | "small" | "medium" | "large" | "xl" | "xxl" | "xxxl";
}

export default function RequiredAsterix({
    size = "medium"
}: RequiredAsterixProps) {

    const sizes = {
        xs: "text-xs",
        small: "text-sm",
        medium: "text-base",
        large: "text-lg",
        xl: "text-xl",
        xxl: "text-2xl",
        xxxl: "text-3xl"
    };

    return (
        <span 
            className={`
                text-error
                ${sizes[size]}
                `}
        >
            *
        </span>
    );
}