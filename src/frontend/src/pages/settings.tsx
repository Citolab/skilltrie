/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { useEffect, useState } from "react";
import {
    ActivateSetting,
    CreateSetting,
    GetActiveSetting,
    GetSettings,
    UpdateSetting,
} from "../api/settings";
import { defaultSetting, type Setting } from "../types/setting";
import SettingCard from "../components/settings/setting-card";
import { toast } from "react-toastify";

export default function Settings() {
    const [activeSetting, setActiveSetting] = useState<Setting | null>(null);

    const [creatingNewSetting, setCreatingNewSetting] = useState<boolean>(false);

    const [newSetting, setNewSetting] = useState<number>(new Date().getTime());
    const [allSettings, setAllSettings] = useState<Setting[]>([]);

    const paginationLimit = 5;
    const [offset, setOffset] = useState<number>(0);

    useEffect(() => {
        invalidate();
    }, []);

    function handlePagination() {
        const newOffset = offset + paginationLimit;

        setOffset(newOffset);
        GetSettings(newOffset, paginationLimit)
            .then((newSettings) => {
                setAllSettings((old) => [...old, ...newSettings]);

                if (newSettings.length === 0) {
                    toast.info("No more settings to load.");
                }
            })
            .catch(() => {
                toast.error("Error: could not retrieve any settings!");
            });
    }

    function invalidate() {
        setOffset(0);
        GetActiveSetting()
            .then(setActiveSetting)
            .catch(() => {
                toast.error("Error: could not retrieve the active setting!");
            });
        GetSettings(0, paginationLimit)
            .then(setAllSettings)
            .catch(() => {
                toast.error("Error: could not retrieve any settings!");
            });
    }

    return (
        <div className="text-white p-4 pr-15 pl-15 gap-8">
            <h1 className="text-4xl font-bold mt-4 mb-8 text-admin-text-LD">Settings</h1>
            <div className="flex 2xl:flex-row-reverse 2xl:justify-between flex-col gap-6 2xl:gap-0">
                <div className="flex flex-col gap-4">
                    <h2 className="text-3xl font-bold text-admin-text-LD">Active Setting</h2>
                    <div className="flex flex-col">
                        {activeSetting && (
                            <SettingCard
                                key={activeSetting.id}
                                className="bg-linear-to-r from-cyan-500 to-primary shadow-lg shadow-cyan-500/50"
                                initialSetting={activeSetting}
                                active={true}
                                onSubmit={async (updatedSetting) => {
                                    // artificial delay
                                    await new Promise((resolve) => setTimeout(resolve, 300));

                                    try {
                                        await UpdateSetting(updatedSetting);

                                        toast.success(
                                            `Setting '${updatedSetting.profileName}' successfully updated!`
                                        );

                                        invalidate();
                                    } catch (error) {
                                        toast.error(
                                            `Something went wrong updating this setting...`
                                        );

                                        throw error;
                                    }
                                }}
                            />
                        )}
                    </div>
                </div>
                <div className="flex flex-col gap-6">
                    <h2 className="text-3xl font-bold text-admin-text-LD">Create Settings</h2>
                    {!creatingNewSetting && (
                        <div
                            className="
                            h-15 bg-black cursor-pointer select-none
                            flex items-center px-4 text-2xl font-bold
                            shadow-lg hover:shadow-white/10 transition-all
                        "
                            onClick={() => setCreatingNewSetting(true)}
                        >
                            + Create
                        </div>
                    )}
                    {creatingNewSetting && activeSetting && (
                        <SettingCard
                            initialSetting={defaultSetting}
                            new={true}
                            submitText="Create Setting"
                            onSubmit={async (updatedSetting) => {
                                // artificial delay
                                await new Promise((resolve) => setTimeout(resolve, 300));

                                try {
                                    await CreateSetting(updatedSetting);

                                    setNewSetting(new Date().getTime());

                                    toast.success("Setting successfully created!");

                                    setCreatingNewSetting(false);

                                    invalidate();
                                } catch (error) {
                                    toast.error(`Something went wrong creating this setting...`);

                                    throw error;
                                }
                            }}
                            key={newSetting}
                        />
                    )}
                    <h2 className="text-3xl font-bold text-admin-text-LD">Other Settings</h2>
                    {allSettings
                        .filter((s) => s.id !== activeSetting?.id)
                        .map((s) => {
                            return (
                                <SettingCard
                                    key={s.id}
                                    className="bg-gray-700"
                                    initialSetting={s}
                                    onSubmit={async (updatedSetting) => {
                                        // artificial delay
                                        await new Promise((resolve) => setTimeout(resolve, 300));

                                        try {
                                            await UpdateSetting(updatedSetting);

                                            toast.success("Setting successfully updated!");

                                            invalidate();
                                        } catch (error) {
                                            toast.error(
                                                `Something went wrong updating this setting...`
                                            );

                                            throw error;
                                        }
                                    }}
                                    onMakeActive={async (setting) => {
                                        // artificial delay
                                        await new Promise((resolve) => setTimeout(resolve, 300));

                                        try {
                                            await ActivateSetting(setting);

                                            toast.success(
                                                `Successfully activated setting '${setting.profileName}'!`
                                            );

                                            invalidate();
                                        } catch (error) {
                                            toast.error(
                                                "Error: Could not make this setting active, please try again later."
                                            );

                                            throw error;
                                        }
                                    }}
                                />
                            );
                        })}
                    <button
                        className="
                            h-12 bg-black cursor-pointer select-none
                            flex items-center px-4 text-xl justify-center
                            shadow-lg hover:shadow-white/10 transition-all
                        "
                        onClick={handlePagination}
                    >
                        Load more
                    </button>
                </div>
            </div>
        </div>
    );
}
