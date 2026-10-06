/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { FetchJson } from "../utils/fetch-json";
const controllerUrl = "/api/qti/";

export function GetItem(id: number) {
    return FetchJson<string>(controllerUrl.concat(`get-item/${id}`));
}

export function GetAssessment(ids: number[]) {
    return FetchJson<string>(controllerUrl.concat("assessment"), {
        method: "POST",
        body: JSON.stringify(ids),
        headers: { "Content-Type": "application/json" },
    });
}
