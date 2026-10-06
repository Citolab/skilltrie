/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { useState } from "react";

interface AdminCardProps {
    children: React.ReactNode;
    title: string;
    defaultOpen?: boolean;
}

function AdminCard(props: AdminCardProps) {
    const [open, setOpen] = useState<boolean>(props.defaultOpen ?? false);

    return (
        <div>
            <div
                className={`
                    bg-gray-700 hover:bg-gray-600 
                    transition-colors duration-150 
                    flex items-center space-x-2 p-4
                    cursor-pointer
                    ${open ? "rounded-t-lg" : "rounded-lg"}
                `}
                onClick={() => setOpen(!open)}
            >
                <svg
                    className={`w-6 h-6 ${open ? "rotate-90" : ""}`}
                    fill="none"
                    stroke="white"
                    strokeWidth="2"
                    viewBox="0 0 24 24"
                    xmlns="http://www.w3.org/2000/svg"
                >
                    <path strokeLinecap="round" strokeLinejoin="round" d="M9 5l7 7-7 7" />
                </svg>
                <h1 className="text-2xl text-white font-medium select-none">{props.title}</h1>
            </div>
            <div
                className={`${open ? "" : "hidden"} bg-white text-black px-4 pb-4 pt-1 rounded-b-lg`}
            >
                {props.children}
            </div>
        </div>
    );
}

export default AdminCard;
