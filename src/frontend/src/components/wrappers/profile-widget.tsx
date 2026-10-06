/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { twMerge } from "tailwind-merge";

interface ProfileWidgetProps {
    children: React.ReactNode;
    className?: string;
    style?: React.CSSProperties;
}
export function ProfileWidget({ children, className, style }: ProfileWidgetProps) {
    return (
        <div
            className={twMerge(`flex border-2 border-borderDefault rounded-2xl`, className)}
            style={style}
        >
            {children}
        </div>
    );
}
