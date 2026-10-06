/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { type CharacterType } from "@/components/customisation/characters/character-display";
import { type PaletteName } from "@/assets/characters/palettes";

// Mirrors the backend UserCharacterDTO.
export interface UserCharacter {
    id: number;
    name: string;
    selected: boolean;
    palette: string;
}

export const defaultUserCharacter: UserCharacter = {
    id: 1,
    name: "cat",
    selected: false,
    palette: "classic",
};

// Mirrors the backend CharacterDTO
export interface Character {
    id: number;
    name: string;
}

export interface ResolvedCharacter {
    character: CharacterType;
    palette: PaletteName;
}

export function ResolveCharacter(userCharacter: UserCharacter): ResolvedCharacter {
    return {
        character: userCharacter.name as CharacterType,
        palette: userCharacter.palette as PaletteName,
    };
}
