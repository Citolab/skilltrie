/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

export interface Cosmetic {
    id: number;
    name: string;
    price: number;
    currencyId: number;
    type: ClothingType;
    riveFile: string;
    riveArtboard: string;
    riveStateMachine: string;
    riveInput: string;
    riveInputValue: number;
    iconFile: string;
}

export interface UserCosmetic {
    userId?: number;
    cosmeticId: number;
    equipped: boolean;
}

export type ClothingType = "Hair" | "Hat" | "Glasses" | "Shirt" | "Pants" | "Shoes";
