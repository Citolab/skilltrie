/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import type { UserCharacter, Character } from "../types/character.ts";
import { FetchJson } from "../utils/fetch-json.ts";

const controllerUrl = "/api/characters/";

/**
 * Fetches the character that the user with the given id currently has selected.
 */
export function GetSelectedUserCharacter(userId: number) {
    return FetchJson<UserCharacter>(controllerUrl.concat(`${userId}/selected`));
}

/**
 * Fetches the characters that the user with the given id has.
 */
export function GetUserCharacters() {
    return FetchJson<UserCharacter[]>(controllerUrl.concat(`user`));
}

/**
 * Fetches all the characters from the database
 */
export function GetCharacters() {
    return FetchJson<Character[]>(controllerUrl.slice(0, -1));
}

export function UpdateCharacter(character: UserCharacter) {
    return FetchJson<void>(controllerUrl.concat(`user/selected`), {
        method: "PUT",
        body: JSON.stringify(character),
        headers: { "Content-Type": "application/json" },
    });
}
