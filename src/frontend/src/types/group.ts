/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import type { User } from "./user.ts";

export interface GroupMember {
    id: number;
    userId: number;
    user: User; }

export interface Group {
    id: number;
    name: string;
    members: GroupMember[];
}

export const defaultGroup: Group = {
    id: 0,
    name: "",
    members: [],
};