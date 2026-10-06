/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

//the user type that we use in the database
export interface User {
    id: number;
    role: string;
    firstName: string;
    infix: string | null;
    lastName: string;
    displayName: string;
    email: string;
    password: string | null;
}

export const defaultUser: User = {
    id: 0,
    displayName: "",
    firstName: "",
    infix: null,
    lastName: "",
    role: "",
    email: "",
    password: null,
};

export interface PublicUser {
    id: number;
    role: string;
    displayName: string;
}

export const defaultPublicUser: PublicUser = {
    id: 0,
    displayName: "",
    role: "",
};
