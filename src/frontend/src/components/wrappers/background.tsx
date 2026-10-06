/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import React from "react";

interface BackgroundWrapperProps {
    backgroundImage: string;
    children: React.ReactNode;
}

export default function BackgroundWrapper({ backgroundImage, children }: BackgroundWrapperProps) {
    return (
        <div>
            <div
                className="fixed h-screen w-full bg-cover -z-100"
                style={{ backgroundImage: `url(${backgroundImage})` }}
            ></div>
            {children}
        </div>
    );
}
