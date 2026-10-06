/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import React from "react";

interface ShowPasswordCheckboxProps {
    checked: boolean;
    onChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
}

export default function ShowPasswordCheckbox({ checked, onChange }: ShowPasswordCheckboxProps) {
    return (
        <div className="w-[70%] flex items-center justify-center space-x-2 text-black group cursor-pointer">
            <input
                type="checkbox"
                id="showpassword"
                name="showpassword"
                checked={checked}
                onChange={onChange}
                className="cursor-pointer"
            />
            <label
                htmlFor="showpassword"
                className="text-base transition-colors duration-200 group-hover:text-gray-700"
            >
                Show password
            </label>
        </div>
    );
}
