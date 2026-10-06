/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

export interface UserBadge {
    progress?: number;
    progressNeeded: number;
    name?: string;
    identifier: string;
    description?: string;
    category?: string;
    stampImage?: string;
    stamp?: UserStamp;
}

export interface UserStamp {
    x: number;
    y: number;
    page: number;
    dateAccomplished: string;
}
