/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { FetchJson } from "../utils/fetch-json.ts";

const controllerUrl = "/api/tour/";

export function GetTourSeen(tourKey: string): Promise<boolean> {
    return FetchJson<boolean>(controllerUrl.concat(tourKey));
}

export function MarkTourSeen(tourKey: string): Promise<void> {
    return FetchJson<void>(controllerUrl.concat(tourKey), {
        method: "POST",
    });
}