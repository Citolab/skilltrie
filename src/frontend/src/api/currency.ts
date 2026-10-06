/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { FetchJson } from "../utils/fetch-json.ts";
import { type Currency, type UserCurrency, type UserCurrencyUpdateDto } from "../types/currency.ts";

const controllerUrl = "/api/currency/";

export function GetCurrencies() {
    return FetchJson<Currency[]>(controllerUrl.concat(`currencies`));
}

export function GetCurrency(id: number) {
    return FetchJson<Currency[]>(controllerUrl.concat(`currency/${id}`));
}

export function GetUserCurrencies(userId?: number) {
    const params = new URLSearchParams();

    // Admin users requesting currency data of other users
    if (userId) {
        params.append("userId", userId.toString());
    }

    const query = params.toString();
    const url = controllerUrl + "usercurrency" + (query ? `?${query}` : "");

    return FetchJson<UserCurrency[]>(url);
}

export function UpdateUserCurrency(data: UserCurrencyUpdateDto, userId?: number) {
    const params = new URLSearchParams();

    // Admin users updating cosmetic data of other users
    if (userId) {
        params.append("userId", userId.toString());
    }

    const query = params.toString();
    const url = controllerUrl + "usercurrency/update" + (query ? `?${query}` : "");
    return (
        FetchJson<UserCurrency>(url),
        {
            method: "PUT",
            body: JSON.stringify(data),
            headers: { "Content-Type": "application/json" },
        }
    );
}
