/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { useCallback, useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import type { Group } from "../../../types/group";
import {
    GetGroup,
    GetGroupProficiency,
    AddGroupMember,
    RemoveGroupMember,
} from "../../../api/group";
import AdminButton from "../../../components/admin-dashboard/admin-input/admin-button";
import AdminTable from "../../../components/admin-dashboard/admin-table/admin-table";
import PopupWindow from "../../../components/admin-dashboard/admin-popup-window";
import ObjectForm from "../../../components/admin-dashboard/admin-generic-object-form";
import type { User } from "../../../types/user.ts";

export default function GroupPage() {
    const { id } = useParams();
    const groupId = Number(id);

    const [group, setGroup] = useState<Group | null>(null);
    const [proficiency, setProficiency] = useState<number>(0);

    const [addMemberOpen, setAddMemberOpen] = useState(false);

    const refresh = useCallback(async () => {
        const [g, p] = await Promise.all([GetGroup(groupId), GetGroupProficiency(groupId)]);
        setGroup(g);
        setProficiency(p.averageProficiency);
    }, [groupId]);

    const objectKeys: (keyof User)[] = ["email", "displayName", "firstName", "infix", "lastName"];

    useEffect(() => {
        refresh().catch(console.error);
    }, [groupId, refresh]);

    if (!group) return <div className="text-white p-4">Loading…</div>;

    return (
        <div className="dark:text-white">
            <h1 className="p-4 text-4xl font-bold mb-4">{group.name}</h1>

            {/* Proficiency */}
            {proficiency != 0 && (
                <div className="p-4 rounded mb-6">
                    <h2 className="text-xl font-semibold mb-2">Group Proficiency</h2>
                    <div className="grid grid-cols-3 gap-4">
                        <div>Average: {proficiency}</div>
                    </div>
                </div>
            )}

            {/* Members */}
            <div className=" p-4 rounded mb-6">
                <div className="flex justify-between items-center mb-2">
                    <h2 className="text-xl font-semibold">Members</h2>
                    <AdminButton onClick={() => setAddMemberOpen(true)}>+ Add Member</AdminButton>
                </div>
                <AdminTable
                    rows={group.members.map((m) => m.user)}
                    objectKeys={objectKeys}
                    headerNames={{ firstName: "first name", lastName: "last name" }}
                    tableActions={{
                        delete: {
                            action: (user) => {
                                void RemoveGroupMember(groupId, user.id).then(refresh);
                            },
                        },
                    }}
                />
            </div>

            {/* Add Member Popup */}
            <PopupWindow
                isOpen={addMemberOpen}
                onClose={() => setAddMemberOpen(false)}
                title="Add Member"
            >
                <ObjectForm
                    initial={{ id: 0 }}
                    commitText="Add"
                    onCancel={() => setAddMemberOpen(false)}
                    onSubmit={(member) => {
                        void AddGroupMember(groupId, member.id).then(() => {
                            setAddMemberOpen(false);
                            refresh().catch(console.error);
                        });
                    }}
                    inputs={{
                        id: {
                            label: "Member ID",
                            constraint: { required: true },
                        },
                    }}
                />
            </PopupWindow>
        </div>
    );
}
