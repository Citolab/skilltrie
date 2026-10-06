/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import RiveButton from "./rive-button";

/**
 * temporary wrapper around the rive button
 */
function RiveButtonWrapper() {
    return (
        <>
            <div>
                {/* This code is intened as a hotfix, don't (re)use it or anything */}
                <div className="relative overflow-hidden w-[400px] aspect-video">
                    <div className="h-full cursor-pointer">
                        <RiveButton />
                    </div>
                    <div className="absolute top-0 w-full h-[19%] bg-transparent" />
                    <div className="absolute bottom-0 w-full h-[22%] bg-transparent" />
                    <div className="absolute top-0 left-0 w-[19%] h-full bg-transparent" />
                    <div className="absolute top-0 right-0 w-[19%] h-full bg-transparent" />
                </div>
            </div>
        </>
    );
}

export default RiveButtonWrapper;
