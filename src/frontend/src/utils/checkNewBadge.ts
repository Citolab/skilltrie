/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import type { UserBadge } from "../types/badge.ts";
import { GetUserBadges } from "../api/badge";

export default async function CheckNewBadge(): Promise<UserBadge[] | null> {
    //get DB values
    const currentBadges = await GetUserBadges().then((badges) => badges.filter(b => b.stamp != null));
    const currentBadgesCount = currentBadges.length;

    //get memory values
    const oldBadgesCount = sessionStorage.getItem("badgesCount");
    const oldBadgesJSON = sessionStorage.getItem("badges");

    sessionStorage.setItem("badgesCount", currentBadgesCount.toString());
    sessionStorage.setItem("badges", JSON.stringify(currentBadges.map(b => b.identifier)));

    // First visit, return null
    if (!oldBadgesCount || !oldBadgesJSON) return null;

    //returns the new badges if there are new badges
    if (Number(oldBadgesCount) < currentBadgesCount) {
        const memoryBadges: string[] = JSON.parse(oldBadgesJSON);
        return currentBadges.filter(x => !memoryBadges.some(y => x.identifier === y))
    }

    return null
}
