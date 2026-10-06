/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import type { User, PublicUser } from "../types/user";
import { FetchJson } from "../utils/fetch-json.ts";
const controllerUrl = "/api/users/";

//gets all the users in an array
export function GetUsers(offset: number, range: number) {
    const params = new URLSearchParams({
        offset: String(offset),
        range: String(range),
    });

    return FetchJson<User[]>(controllerUrl.concat(`users?${params.toString()}`));
}

export function GetActiveUser() {
    return FetchJson<User>(controllerUrl.concat("user"));
}

//gets a single user
export function GetUser(id: number) {
    return FetchJson<User>(controllerUrl.concat(`user/${id}`));
}

export function GetPublicUserByName(username: string) {
    return FetchJson<PublicUser>(controllerUrl.concat(`username/${username}`));
}

//check if user is an admin
export function UserIsAdmin() {
    return FetchJson<boolean>(controllerUrl.concat(`user/isAdmin`));
}

//creates a user based on the userinfo send
export function CreateUser(data: User) {
    return FetchJson<void>(controllerUrl.concat("add"), {
        method: "POST",
        body: JSON.stringify(data),
        headers: { "Content-Type": "application/json" },
    });
}

//update a user by giving an id and the info you want to update
export function UpdateUser(id: number, data: User) {
    return FetchJson<void>(controllerUrl.concat(`update/${id}`), {
        method: "PUT",
        body: JSON.stringify(data),
        headers: { "Content-Type": "application/json" },
    });
}

//delete a user by giving the id
export function DeleteUser(id: number) {
    return FetchJson<void>(controllerUrl.concat(`delete/${id}`), {
        method: "DELETE",
    });
}
