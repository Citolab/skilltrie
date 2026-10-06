/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

export interface Setting {
    id: number;
    timeBeforeRedo: string; // these are ISO 8601 timespan strings
    profileName: string;
    lastActive: string;
    levelSize: number;
    aiFactor: number;
}

export const defaultSetting: Partial<Setting> = {
    timeBeforeRedo: "",
    profileName: "",
    levelSize: 10,
    aiFactor: 0.2,
};
