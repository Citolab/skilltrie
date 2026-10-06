/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { useEffect, useState } from "react";
import { PopupLayout } from "../../general-components/popup-layout.tsx";
import { ProgressBar } from "../../../components/general-components/general-progress-bar.tsx";

interface BadgeProps {
    image: string;
    name: string;
    requirement?: string;
    unlocked: boolean;
    unlockDate?: string;
    progress: number;
    progressNeeded: number;
}

export default function BadgeDetails({
    open,
    badge,
    onClose,
}: {
    open: boolean;
    badge: BadgeProps | null;
    onClose: () => void;
}) {
    const [showedBadge, setShowedBadge] = useState(badge); //make sure the active badge is stored, instead of it immidiately being set to null when the popup is closed

    useEffect(() => {
        if (badge != null) setShowedBadge(badge);
    }, [badge]);

    return (
        <PopupLayout
            open={open}
            title={showedBadge?.unlocked ? "Badge Accomplished" : "Badge Locked"}
            headerClassName={showedBadge?.unlocked ? "yellow" : "gray"}
            onClose={onClose}
        >
            <div className="flex flex-col">
                <div className="m-auto flex-col w-full">
                    {" "}
                    <img
                        src={showedBadge?.image}
                        alt="Achievement image"
                        className={`self-center m-auto text-center w-16 ${!showedBadge?.unlocked && "grayscale"}`}
                    />
                    <p className="line-clamp-2 text-center text-sm">{showedBadge?.name}</p>
                </div>
                <p>{showedBadge?.requirement}</p>{" "}
                {showedBadge?.progressNeeded && showedBadge?.progressNeeded > 1 && (
                    <ProgressBar
                        progress={showedBadge?.progress}
                        progressNeeded={showedBadge?.progressNeeded}
                    />
                )}
            </div>
        </PopupLayout>
    );
}
