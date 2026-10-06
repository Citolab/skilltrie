/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

export interface ParameterDTO {
    name: string;
    type: string;
}

export type ParameterizedBadges = Record<string, ParameterDTO[]>;

export type UpdateBadgeStateDTO = {
    identifier: string;
    openFrom?: Date;
    openUntil?: Date;
    flagKey?: string;
    flagVariant?: string;
};

export type BadgeInfo = {
    identifier: string;
    name?: string;
    description?: string;
    category?: string;
    stamp?: string;
    openFrom?: string;
    openUntil?: string;
    flagKey?: string;
    flagVariant?: string;
    phase: "Staging" | "Published" | "Disabled";
};
