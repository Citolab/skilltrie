/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import type { UserBadge } from "../types/badge.ts";
import { FetchJson } from "../utils/fetch-json.ts";

const controllerUrl = "/api/badges/";

/**
 * Fetches all the badges of the currently logged in user
 * */
export function GetUserBadges() {
    return FetchJson<UserBadge[]>(controllerUrl);
}

/**
 * Fetches all the badges of the user with the given id.
 * */
export function GetBadgesByUserId(userId: number) {
    return FetchJson<UserBadge[]>(controllerUrl.concat(`${userId}`));
}
