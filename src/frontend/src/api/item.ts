/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import type { Item, SmallItemDto } from "../types/item.ts";
import { FetchJson } from "../utils/fetch-json.ts";
const controllerUrl = "/api/items/";

/** get items, paginated
 *
 * @returns `Promise<SmallItemDto[]>`
 */
export function GetItems(offset: number, range: number, sortColumn: string, sortOrder: string) {
    const params = new URLSearchParams({
        offset: String(offset),
        range: String(range),
        sortColumn,
        sortOrder,
    });
    return FetchJson<SmallItemDto[]>(controllerUrl.concat(`items?${params.toString()}`));
}

/** get a single item */
export function GetItem(id: number) {
    return FetchJson<Item>(controllerUrl.concat(`item/${id}`));
}

/** update a single item */
export function UpdateItem(data: Item) {
    return FetchJson<Item>(controllerUrl.concat("update"), {
        method: "PUT",
        body: JSON.stringify(data),
        headers: { "Content-Type": "application/json" },
    });
}

/** search items on the backend by their question text */
export function SearchItemsByQuestionText(searchString: string) {
    const params = new URLSearchParams({
        searchString: searchString,
    });

    return FetchJson<SmallItemDto[]>(
        controllerUrl.concat(`searchbyquestiontext?${params.toString()}`),
        {
            headers: { accept: "application/json" },
        }
    );
}

/** deactivate an item by providing an Id */
export function DeactivateItem(id: number) {
    return FetchJson<void>(controllerUrl.concat("deactivate/") + id);
}

/** activate an item by providing an Id */
export function ActivateItem(id: number) {
    return FetchJson<void>(controllerUrl.concat("activate/") + id);
}

/** remove all reports for an item by providing an Id */
export function ResolveReports(id: number) {
    return FetchJson<void>(controllerUrl.concat("resolve/") + id)
}