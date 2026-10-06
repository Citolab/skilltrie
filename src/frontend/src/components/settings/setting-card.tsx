/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { useState } from "react";
import { type Setting } from "../../types/setting";
import * as luxon from "luxon";

interface SettingCardProps {
    submitText?: string;
    className?: string;
    initialSetting: Partial<Setting>;
    new?: boolean;
    active?: boolean;
    onMakeActive?: (setting: Partial<Setting>) => Promise<void>;
    onSubmit?: (updated: Partial<Setting>) => Promise<void>;
}

// eslint-disable-next-line @typescript-eslint/naming-convention
function SettingCardListing(props: { children: React.ReactNode }): React.ReactElement {
    return <div className="grid grid-cols-[300px_1fr] gap-x-4 px-4 py-1.5">{props.children}</div>;
}

function SettingCard(props: SettingCardProps) {
    const [initialSetting, setInitialSetting] = useState<Partial<Setting>>(props.initialSetting);
    const [edit, setEdit] = useState<boolean>(props.new ?? false);
    const [saving, setSaving] = useState<boolean>(false);

    function updateField<K extends keyof Setting>(field: K, value: Setting[K]) {
        setInitialSetting((prev) => ({ ...prev, [field]: value }));
    }

    function returnUpdatedSetting() {
        props
            .onSubmit?.(initialSetting)
            .then(() => {
                setEdit(false);
                setSaving(false);
            })
            .catch(() => {
                setEdit(true);
                setSaving(false);
            });
    }

    function handleSetActive() {
        setSaving(true);
        props
            .onMakeActive?.(props.initialSetting)
            .then(() => {
                setSaving(false);
            })
            .catch(() => {
                setSaving(false);
            });
    }

    const inputStyle = edit ? "outline py-2 px-1" : "pointer-events-none";
    const labelStyle = "flex items-center";

    function numberInput(
    field: keyof Setting,
    value: number,
    { min, max, isFloat }: { min: number; max?: number; isFloat?: boolean }
) {
    const parse = isFloat ? parseFloat : parseInt;
    return {
        type: edit ? "number" : "text",
        value: value ?? min,
        onChange: (e: React.ChangeEvent<HTMLInputElement>) => {
            const raw = e.target.value;
            if (raw === "" || raw === "0" || (isFloat && raw === "0.")) {
                updateField(field, raw as any);
                return;
            }
            if (/^0{2,}/.test(raw)) return;
            const num = parse(raw, isFloat ? undefined : 10);
            if (!isNaN(num)) updateField(field, max !== undefined ? Math.min(max, Math.max(min, num)) : Math.max(min, num));
        },
        onBlur: (e: React.ChangeEvent<HTMLInputElement>) => {
            const val = Number(e.target.value);
            updateField(field, isNaN(val) ? min : max !== undefined ? Math.min(max, Math.max(min, val)) : Math.max(min, val));
        },
        className: inputStyle,
    };
}

    return (
        <div
            className={`bg-black shadow-xl ${saving ? "opacity-50 **:pointer-events-none **:select-none cursor-progress" : ""}`}
        >
            <div className="flex justify-between border-b border-b-white bg-lack">
                <h2 className="text-xl p-4 max-w-75 overflow-hidden truncate">
                    {initialSetting.profileName === "" ? "New Profile" : initialSetting.profileName}
                </h2>
                {!props.new && (
                    <div className="cursor-pointer flex items-stretch *:px-4 *:hover:bg-[rgb(5,5,5)]">
                        {!props.active && (
                            <div onClick={handleSetActive} className="font-bold flex items-center">
                                Set Active
                            </div>
                        )}
                        <div className="flex items-center" onClick={() => setEdit(!edit)}>
                            <svg
                                xmlns="http://www.w3.org/2000/svg"
                                viewBox="0 0 48 48"
                                width="24"
                                height="24"
                                fill="currentColor"
                            >
                                <g id="pencil">
                                    <path d="M46.84,5.32,42.68,1.16a4,4,0,0,0-5.58,0C1.7,36.55,3.65,34.52,3.53,34.88S3,36.78,0,46.72A1,1,0,0,0,1,48c.21,0,12.08-3.45,12.39-3.68S10.64,47.11,46.84,10.9A4,4,0,0,0,46.84,5.32ZM35,6.05,42,13l-1.37,1.37L33.66,7.42ZM10.45,38.91l-1-.34-.34-1L35,11.61,36.39,13ZM32.25,8.83l1.36,1.37L7.79,36,6.08,35ZM3.32,42.67a7.68,7.68,0,0,1,2,2l-2.85.84Zm4,1.42a9.88,9.88,0,0,0-3.43-3.43l1.16-3.94,2,1.23c.88,2.62.38,2.08,2.94,2.94l1.23,2ZM13,41.92l-1-1.71L37.8,14.39l1.37,1.36ZM45.43,9.49l-2.07,2.07L36.44,4.64l2.07-2.07a1.94,1.94,0,0,1,2.75,0l4.17,4.17A1.94,1.94,0,0,1,45.43,9.49Z" />
                                </g>
                            </svg>
                        </div>
                    </div>
                )}
            </div>
            <div>
                <div
                    className={`
                            flex flex-col
                            [&>*:nth-child(even)]:bg-[rgba(255,255,255,0.2)]
                            [&>*:nth-child(odd)]:bg-[rgba(255,255,255,0.3)]
                            ${props.className ?? ""}
                        `}
                >
                    <SettingCardListing>
                        <div className={labelStyle}>Profile Name</div>
                        <input
                            value={initialSetting.profileName}
                            onChange={(e) => updateField("profileName", e.target.value)}
                            className={inputStyle}
                        />
                    </SettingCardListing>
                    <SettingCardListing>
                        <div className={labelStyle}>Time Before Question Redo</div>
                        <input
                            value={initialSetting.timeBeforeRedo}
                            onChange={(e) => updateField("timeBeforeRedo", e.target.value)}
                            className={inputStyle}
                            placeholder="HH:MM:SS"
                        />
                    </SettingCardListing>
                    <SettingCardListing>
                        <div className={labelStyle}>Number of Questions</div>
                        <input 
                            min="1" {...numberInput("levelSize", initialSetting.levelSize ?? 10, { min: 1 })} />
                    </SettingCardListing>
                    <SettingCardListing>
                        <div className={labelStyle}>AI factor</div>
                        <input 
                            min="0" 
                            max="1" 
                            step="0.1" {...numberInput("AiFactor", initialSetting.AiFactor ?? 0.2, { min: 0, max: 1, isFloat: true })} 
                        />
                    </SettingCardListing>
                    {!props.new && ( // don't render if new card
                        <SettingCardListing>
                            <div className={labelStyle}>
                                {props.active ? "Active Since" : "Last Active"}
                            </div>
                            <input
                                value={
                                    luxon.DateTime.fromISO(initialSetting.lastActive ?? "").toISO({
                                        includeOffset: false,
                                    }) ?? "invalid date-time"
                                }
                                type="datetime-local"
                                onChange={(e) => updateField("lastActive", e.target.value)}
                                disabled
                            />
                        </SettingCardListing>
                    )}
                </div>
            </div>
            <div
                className={`
                        ${edit ? "" : "hidden"}
                        p-2 text-center font-bold cursor-pointer hover:bg-[rgb(5,5,5)]
                    `}
                onClick={() => {
                    setEdit(false);
                    setSaving(true);
                    returnUpdatedSetting();
                }}
            >
                {props.submitText ?? "Save"}
            </div>
        </div>
    );
}

export default SettingCard;
