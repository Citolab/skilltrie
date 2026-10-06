/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import type { UserStreak } from "../types/streak.ts";
import { FetchJson } from "../utils/fetch-json.ts";

const controllerUrl = "/api/users/";

/**
 * Fetches the streak data (current and highest) of the user with the given id.
 */
export function GetUserStreak(userId: number) {
    return FetchJson<UserStreak>(controllerUrl.concat(`${userId}/streak`));
}
