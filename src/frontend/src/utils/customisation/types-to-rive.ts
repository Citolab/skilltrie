/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { type Cosmetic, type UserCosmetic } from "../../types/cosmetic";

export function OwnedCosmetics(allCos: Cosmetic[], userCos: UserCosmetic[]): Cosmetic[] {
    const owned = new Set(userCos.map((uc) => uc.cosmeticId));
    return allCos.filter((c) => owned.has(c.id));
}

export function EquippedCosmetics(allCos: Cosmetic[], userCos: UserCosmetic[]): Cosmetic[] {
    const equipped = new Set(userCos.filter((uc) => uc.equipped).map((uc) => uc.cosmeticId));
    return allCos.filter((c) => equipped.has(c.id));
}
