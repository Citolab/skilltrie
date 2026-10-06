/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import type { NodeTooltipStyling } from "./node-tooltip.tsx";
import type { UserScopeInfo } from "../../../types/scope.ts";
import { LockClosedIcon, LockOpenIcon, StarIcon } from '@heroicons/react/24/solid'

const masteredStyling: NodeTooltipStyling = {
    tooltipColor: "var(--color-green-500)",
    buttonColor: "var(--color-green-800)",
    hoverButtonColor: "var(--color-green-700)",
    mainTextColor: "var(--color-black)",
    subTextColor: "var(--color-gray-800)",
    icon: StarIcon,
    iconColor: "var(--color-yellow-200)"
};

const unlockedStyling: NodeTooltipStyling = {
    tooltipColor: "var(--color-blue-300)",
    buttonColor: "var(--color-blue-600)",
    hoverButtonColor: "var(--color-blue-500)",
    mainTextColor: "var(--color-black)",
    subTextColor: "var(--color-gray-800)",
    icon: LockOpenIcon,
    iconColor: "var(--color-gray-700)"
};

const lockedStyling: NodeTooltipStyling = {
    tooltipColor: "var(--color-gray-500)",
    buttonColor: "var(--color-gray-800)",
    hoverButtonColor: "var(--color-gray-800)",
    buttonText: "Level Locked",
    buttonDisabled: true,
    mainTextColor: "var(--color-white)",
    subTextColor: "var(--color-gray-300)",
    icon: LockClosedIcon,
};

export const getTooltipStyling: (topicInfo: UserScopeInfo) => NodeTooltipStyling = (
    topicInfo
) => {
    return topicInfo.mastered
        ? masteredStyling
        : topicInfo.available
          ? unlockedStyling
          : lockedStyling;
};
