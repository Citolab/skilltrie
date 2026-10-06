/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { FetchJson } from "../utils/fetch-json.ts";
import type { Group,  } from "../types/group";
import type { User } from "../types/user.ts";
const controllerUrl = "/api/groups";

// Create a new group
export async function CreateGroup(name: string) {
    return FetchJson<Group>(`${controllerUrl}/add`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ name }),   // <-- FIXED
    });
}

// Get all groups with pagination
export async function GetGroups(offset: number = 0, range: number = 50) {
    const params = new URLSearchParams({
        offset: String(offset),
        range: String(range),
    });
    return FetchJson<Group[]>(`${controllerUrl}?${params.toString()}`);
}

// Get a group by id
export async function GetGroup(id: number) {
    return FetchJson<Group>(`${controllerUrl}/${id}`);
}

// Update a group
export async function UpdateGroup(id: number, name: string) {
    return FetchJson<Group>(`${controllerUrl}/${id}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ name }),   // <-- FIXED
    });
}

// Delete a group
export async function DeleteGroup(id: number) {
    return FetchJson<void>(`${controllerUrl}/${id}`, {
        method: "DELETE",
    });
}

// Add a user to a group
export async function AddGroupMember(groupId: number, userId: number) {
    return FetchJson<User>(`${controllerUrl}/${groupId}/members/${userId}`, {
        method: "POST",
    });
}

// Remove a user from a group
export async function RemoveGroupMember(groupId: number, userId: number) {
    return FetchJson<void>(`${controllerUrl}/${groupId}/members/${userId}`, {
        method: "DELETE",
    });
}

// Get average proficiency for a group
export async function GetGroupProficiency(
    groupId: number,
    topicId?: number,
    topicIds?: number[]
) {
    const params = new URLSearchParams();
    if (topicId !== undefined) {
        params.append("topicId", String(topicId));
    }
    if (topicIds && topicIds.length > 0) {
        topicIds.forEach((id) => params.append("topicIds", String(id)));
    }
    const url = params.toString()
        ? `${controllerUrl}/${groupId}/proficiency?${params.toString()}`
        : `${controllerUrl}/${groupId}/proficiency`;

    return FetchJson<{
        groupId: number;
        topicFilter: string;
        averageProficiency: number;
    }>(url);
}

//get weekly score PoC
export async function GetScoreGroup(group: number) {
    const params = new URLSearchParams({
        groupId: String(group),
        totalWeeks: String(4),
    });
    return FetchJson<number[]>(controllerUrl.concat(`/getScore?${params.toString()}`));
}