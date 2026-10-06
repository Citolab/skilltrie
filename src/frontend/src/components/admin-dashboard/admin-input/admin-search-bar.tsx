/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { useState } from "react";

interface AdminSearchBarProps {
    label: string;
    initialValue?: string;
    placeholder?: string;
    onChange: (value: string) => void;
}

function AdminSearchBar(props: AdminSearchBarProps) {
    const [searchString, setSearchString] = useState<string>(props.initialValue ?? "");

    return (
        <form
            className="flex items-center w-80 border rounded bg-admin-input border-admin-border--inactive focus-within:ring-2 focus-within:ring-borderActive"
            onSubmit={(e) => {
                e.preventDefault();
                props.onChange(searchString);
            }}
        >
            <button
                type="button"
                onClick={() => {
                    props.onChange(searchString);
                }}
                className="select-none cursor-pointer capitalize px-4 py-2 text-sm rounded-l border border-borderActive bg-primary text-admin-button-text-LD flex items-center"
            >
                {props.label}
            </button>

            <div className="flex items-center border rounded-r bg-admin-input border-admin-border--inactive w-full">
                <input
                    type="text"
                    className="w-full px-3 py-2 text-sm outline-none bg-inherit text-admin-text-LD placeholder-text-muted"
                    placeholder={props.placeholder ?? ""}
                    defaultValue={props.initialValue ?? ""}
                    onChange={(e) => {
                        const value = e.target.value;

                        setSearchString(value);
                    }}
                />
                <svg
                    xmlns="http://www.w3.org/2000/svg"
                    className="mr-3 w-4 h-4 text-gray-400 shrink-0"
                    aria-hidden="true"
                    fill="none"
                    viewBox="0 0 20 20"
                >
                    <path
                        stroke="currentColor"
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        strokeWidth="2"
                        d="m19 19-4-4m0-7A7 7 0 1 1 1 8a7 7 0 0 1 14 0Z"
                    />
                </svg>
            </div>
        </form>
    );
}

export default AdminSearchBar;
