/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { type UserBadge } from "../../types/badge"
import { Badge, mapBadge } from "../profile/badges"
import { PopupLayout } from "../general-components/popup-layout"

interface NewBadgePopupProps {
    badges: UserBadge[] | null;
    onClose: () => void;
}

export default function NewBadgePopup({ badges, onClose }: NewBadgePopupProps) {
    return (
        <PopupLayout
            open={badges != null}
            title="New badges earned!"
            headerClassName="primary"
            primaryAction={{ label: "continue", onClick: onClose }}
            onClose={onClose}
            dynamicSizing={true}
        >
            <div className="flex flex-col justify-center items-center w-full min-w-0 select-none">
                <div className="flex flex-row justify-center items-stretch p-2 overflow-x-auto whitespace-nowrap gap-1 w-full min-w-0">
                    {badges?.map((badge, index) => (
                        <div
                            key={index}
                            className="flex flex-shrink-0 min-h-40 min-w-40 justify-center"
                        >
                            {<Badge {...mapBadge(badge)} />}
                        </div>
                    ))}
                </div>
            </div>
        </PopupLayout>
    )
}
