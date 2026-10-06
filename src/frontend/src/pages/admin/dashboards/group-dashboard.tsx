/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { useState } from "react";
import type { Group } from "../../../types/group";
import { defaultGroup } from "../../../types/group";
import { useNavigate } from "react-router-dom";
import { GetGroups, GetGroup, CreateGroup, UpdateGroup, DeleteGroup } from "../../../api/group";
import AdminSearchAndPagination from "../../../components/admin-dashboard/admin-search-and-pagination.tsx";
import AdminButton from "../../../components/admin-dashboard/admin-input/admin-button.tsx";
import AdminTable from "../../../components/admin-dashboard/admin-table/admin-table.tsx";
import PopupWindow from "../../../components/admin-dashboard/admin-popup-window.tsx";
import ObjectForm from "../../../components/admin-dashboard/admin-generic-object-form.tsx";
import normalizeDto from "../../../utils/normalize-dto.ts";

export default function GroupsDashboard() {
    const [groups, setGroups] = useState<Group[]>([]);
    const [selectedGroup, setSelectedGroup] = useState<Group | null>(null);
    const [formActive, setFormActive] = useState(false);
    const [groupAction, setGroupAction] = useState<"create" | "edit">("edit");
    const [refreshGroups, setRefreshGroups] = useState<() => void>(() => {});

    const objectKeys: (keyof Group)[] = ["id", "name"];
    const navigate = useNavigate();

    return (
        <div className="w-full bg-admin-surface p-4" data-testid="groups-dashboard">
            <AdminButton
                onClick={() => {
                    setSelectedGroup(defaultGroup);
                    setGroupAction("create");
                    setFormActive(true);
                }}
            >
                + Add Group
            </AdminButton>

            <AdminSearchAndPagination<Group>
                sortOptions={objectKeys}
                endpoints={{
                    pagination: (offset, range) => GetGroups(offset, range),
                }}
                getObjects={(groups) => setGroups(groups)}
                refresh={(refreshFunction) => setRefreshGroups(() => refreshFunction)}
            />

            <AdminTable
                rows={groups}
                objectKeys={objectKeys}
                headerNames={{ id: "ID", name: "Group Name" }}
                tableActions={{
                    view: {
                        action: (group) => {
                            void navigate(`/admin/groups/${group.id}`);
                        },
                    },
                    edit: {
                        action: (group) => {
                            void GetGroup(group.id).then((g) => {
                                setSelectedGroup(g);
                                setGroupAction("edit");
                                setFormActive(true);
                            });
                        },
                    },
                    delete: {
                        action: (group) => {
                            void DeleteGroup(group.id).then(() => {
                                refreshGroups();
                            });
                        },
                    },
                }}
            />

            <PopupWindow
                isOpen={formActive}
                onClose={() => setFormActive(false)}
                title={groupAction === "edit" ? "Edit Group" : "Create Group"}
                size="large"
            >
                <ObjectForm<Group>
                    initial={normalizeDto(selectedGroup, defaultGroup) ?? ({} as Group)}
                    onCancel={() => setFormActive(false)}
                    commitText={groupAction === "edit" ? "Edit" : "Create Group"}
                    onSubmit={(group) => {
                        if (groupAction === "edit") {
                            // backend expects: updateGroup(id, name)
                            void UpdateGroup(group.id, group.name).then(() => refreshGroups());
                        } else {
                            // backend expects: createGroup(name)
                            void CreateGroup(group.name).then(() => refreshGroups());
                        }
                        setFormActive(false);
                    }}
                    inputs={{
                        name: {
                            label: "Group Name",
                            constraint: {
                                required: true,
                                validate(value) {
                                    if (typeof value !== "string") {
                                        return ["Group name must be a string"];
                                    }
                                    if (value.length < 1 || value.length > 255) {
                                        return ["Group name must be 1–255 characters long"];
                                    }
                                    return null;
                                },
                            },
                        },
                    }}
                />
            </PopupWindow>
        </div>
    );
}
