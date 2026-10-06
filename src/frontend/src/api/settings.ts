/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import type { Setting } from "../types/setting.ts";
import { FetchJson } from "../utils/fetch-json.ts";

const controllerUrl = "/api/settings/";

export function GetSettings(offset: number, range: number) {
    const params = new URLSearchParams({
        offset: String(offset),
        range: String(range),
    });

    return FetchJson<Setting[]>(controllerUrl + `get-settings?${params.toString()}`);
}

export function GetActiveSetting() {
    return FetchJson<Setting>(controllerUrl + `active-setting`);
}

export function ActivateSetting(setting: Partial<Setting>) {
    const params = new URLSearchParams({
        id: String(setting.id),
    });

    return FetchJson<void>(controllerUrl + `activate-setting?${params.toString()}`);
}

export function CreateSetting(setting: Partial<Setting>) {
    return FetchJson<void>(controllerUrl + `create`, {
        method: "PUT",
        headers: {
            "content-type": "application/json",
        },
        body: JSON.stringify(setting),
    });
}

export function UpdateSetting(setting: Partial<Setting>) {
    return FetchJson<void>(controllerUrl + `update`, {
        method: "PUT",
        headers: {
            "content-type": "application/json",
        },
        body: JSON.stringify(setting),
    });
}
