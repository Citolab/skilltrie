/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { useState } from "react";
import { GetUsers, UpdateUser, GetUser, DeleteUser, CreateUser } from "../../../api/user";
import { defaultUser, type User } from "../../../types/user";

import AdminTable from "../../../components/admin-dashboard/admin-table/admin-table";
import ObjectForm from "../../../components/admin-dashboard/admin-generic-object-form";
import AdminSearchAndPagination from "../../../components/admin-dashboard/admin-search-and-pagination";
import PopupWindow from "../../../components/admin-dashboard/admin-popup-window";
import normalizeDto from "../../../utils/normalize-dto";
import AdminButton from "../../../components/admin-dashboard/admin-input/admin-button";
import { validateField } from "../../../utils/field-validator.ts";
import { SetErrorText } from "../../../utils/api-error.ts";

export default function UserDashboard() {
    const [users, setUsers] = useState<User[]>([]);

    const [selectedUser, setSelectedUser] = useState<User | null>(null);

    const [formActive, setFormActive] = useState(false);

    const [userAction, setUserAction] = useState<"create" | "edit">("edit");

    const [deleteActive, setDeleteActive] = useState(false);

    const [userToDelete, setUserToDelete] = useState<User | null>(null);

    const [error, setError] = useState<string>("");

    const objectKeys: (keyof User)[] = [
        "email",
        "displayName",
        "firstName",
        "infix",
        "lastName",
        "role",
    ];

    const [refreshUsers, setRefreshUsers] = useState<() => void>(() => {});

    const closeForm =
        (refresh: boolean = true) =>
        () => {
            if (refresh) refreshUsers();
            setError("");
            setFormActive(false);
        };

    return (
        <div className="w-full bg-admin-surface p-4" data-testid="user-dashboard">
            <AdminButton
                onClick={() => {
                    setSelectedUser(defaultUser);
                    setUserAction("create");
                    setFormActive(true);
                }}
            >
                + Add User
            </AdminButton>
            <AdminSearchAndPagination<User>
                sortOptions={objectKeys}
                endpoints={{
                    pagination: (offset, range) => GetUsers(offset, range),
                }}
                getObjects={(users) => setUsers(users)}
                refresh={(refreshFunction) => setRefreshUsers(() => refreshFunction)}
            />
            <AdminTable
                rows={users}
                objectKeys={objectKeys}
                headerNames={{ firstName: "first name", lastName: "last name" }}
                tableActions={{
                    edit: {
                        action: (user) => {
                            void GetUser(user.id).then((user) => {
                                setSelectedUser(user);
                                setUserAction("edit");
                                setFormActive(true);
                            });
                        },
                    },
                    delete: {
                        action: (user) => {
                            setUserToDelete(user);
                            setDeleteActive(true);
                        },
                    },
                }}
            />
            <PopupWindow
                isOpen={formActive}
                onClose={closeForm(false)}
                title={userAction === "edit" ? "edit user" : "create user"}
                size="large"
            >
                <ObjectForm<User>
                    initial={
                        // ensure if the backend returns a partial dto, we add the missing
                        // fields back by using their default values.
                        normalizeDto(selectedUser, defaultUser) ?? ({} as User)
                    }
                    onCancel={closeForm(false)}
                    commitText={userAction === "edit" ? "Save" : "Create user"}
                    errorMessage=""
                    onSubmit={(user) => {
                        if (userAction === "edit") {
                            user.infix = user.infix === "" ? null : user.infix; //change empty strings to null, as the backend interprets empty strings as 'change nothing'
                            void UpdateUser(user.id, user)
                                .then(closeForm())
                                .catch((error) => SetErrorText(error, setError));
                        } else {
                            void CreateUser(user)
                                .then(closeForm())
                                .catch((error) => SetErrorText(error, setError));
                        }
                    }}
                    inputs={{
                        email: {
                            constraint: {
                                required: true,
                                validate: (value) => validateField("email", value),
                            },
                        },
                        displayName: {
                            label: "display name",
                            constraint: {
                                required: true,
                                validate: (value) => validateField("displayName", value),
                            },
                        },
                        firstName: {
                            label: "first name",
                            constraint: {
                                required: true,
                                validate: (value) => validateField("name", value),
                            },
                        },
                        lastName: {
                            label: "last name",
                            constraint: {
                                required: true,
                                validate: (value) => validateField("name", value),
                            },
                        },
                        infix: {
                            constraint: {
                                required: false,
                                validate: (value) => validateField("infix", value),
                            },
                        },
                        role: {
                            inputType: "dropdown",
                            enumOptions: ["User", "Admin"],
                            constraint: { required: true },
                        },
                        password: {
                            constraint: {
                                validate: (value) =>
                                    validateField("password", value, userAction === "edit"), // filling in password is not required for editing a user
                            },
                        },
                    }}
                />
                {error && <p className="text-error">{error}</p>}
            </PopupWindow>
            <PopupWindow
                isOpen={deleteActive}
                onClose={() => setDeleteActive(false)}
                title="Delete user"
                size="small"
            >
                <div className="flex flex-col gap-4 dark:text-white">
                    <p>
                        Are you sure you want to delete <strong>{userToDelete?.email}</strong>? This
                        action cannot be undone.
                    </p>

                    <div className="flex justify-end gap-3">
                        <button
                            className="px-4 py-2 bg-gray-600 text-white rounded hover:cursor-pointer"
                            onClick={() => setDeleteActive(false)}
                        >
                            Cancel
                        </button>

                        <button
                            className="px-4 py-2 bg-red-600 text-white rounded hover:cursor-pointer"
                            onClick={() => {
                                if (!userToDelete) return;

                                void DeleteUser(userToDelete.id).then(() => {
                                    refreshUsers();
                                    setDeleteActive(false);
                                });
                            }}
                        >
                            Delete
                        </button>
                    </div>
                </div>
            </PopupWindow>
        </div>
    );
}
