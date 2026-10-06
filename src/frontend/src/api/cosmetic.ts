/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { FetchJson } from "../utils/fetch-json.ts";
import { type Cosmetic, type UserCosmetic, type ClothingType } from "../types/cosmetic.ts";
const controllerUrl = "/api/cosmetics/";

export function GetCosmetics(clothingType?: ClothingType) {
    const params = new URLSearchParams();
    if (clothingType) {
        params.append("group", String(clothingType));
    }
    const query = params.toString();
    const url = controllerUrl + "cosmetics" + (query ? `?${query}` : "");
    return FetchJson<Cosmetic[]>(url);
}

export function GetCosmeticsByIds(ids: number[]) {
    const params = new URLSearchParams({
        group: String(ids),
    });
    const url = controllerUrl + "cosmetics/ids?" + params.toString();
    return FetchJson<Cosmetic[]>(url);
}

export function GetUserCosmetics(userId?: number, equipped?: boolean, clothingType?: ClothingType) {
    const params = new URLSearchParams();
    if (clothingType) {
        params.append("group", String(clothingType));
    }
    if (equipped) {
        params.append("equipped", String(equipped));
    }
    // Admin users requesting data of other users
    if (userId) {
        params.append("userId", userId.toString());
    }
    const query = params.toString();
    const url = controllerUrl + "usercosmetics" + (query ? `?${query}` : "");

    return FetchJson<UserCosmetic[]>(url);
}

export function EquipUserCosmetic(data: UserCosmetic) {
    return FetchJson(controllerUrl.concat(`usercosmetics/equip`), {
        method: "PUT",
        body: JSON.stringify(data),
        headers: { "Content-Type": "application/json" },
    });
}

export function BuyUserCosmetic(data: UserCosmetic) {
    return FetchJson<UserCosmetic>(controllerUrl.concat(`usercosmetics/buy`), {
        method: "POST",
        body: JSON.stringify(data),
        headers: { "Content-Type": "application/json" },
    });
}
export function SellUserCosmetic(data: UserCosmetic) {
    return FetchJson(controllerUrl.concat(`usercosmetics/sell`), {
        method: "DELETE",
        body: JSON.stringify(data),
        headers: { "Content-Type": "application/json" },
    });
}
