/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { FetchJson } from "@/utils/fetch-json.ts";
import type { BadgeInfo, ParameterizedBadges, UpdateBadgeStateDTO } from "@/types/badge-admin.ts";

const controllerUrl = "/api/admin/badges/";

export function GetParameterizedBadges() {
    return FetchJson<ParameterizedBadges>(controllerUrl.concat(`parameters`));
}

export function AddParameterizedBadge(body: object) {
    return FetchJson(controllerUrl.concat(`parameterized-badge`), {
        method: "POST",
        body: JSON.stringify(body),
        headers: { "Content-Type": "application/json" },
    });
}

export function FetchBadges() {
    return FetchJson<BadgeInfo[]>(controllerUrl, {
        method: "GET",
    });
}

export function ModifyBadgeState(dto: UpdateBadgeStateDTO) {
    return FetchJson(controllerUrl.concat(`state`), {
        method: "PUT",
        body: JSON.stringify(dto),
        headers: { "Content-Type": "application/json" },
    });
}

export function PublishBadge(identifier: string) {
    return FetchJson(controllerUrl.concat("publish/", identifier), {
        method: "PUT",
    });
}

export function RemoveBadge(identifier: string) {
    return FetchJson(controllerUrl.concat(identifier), {
        method: "DELETE",
    });
}
