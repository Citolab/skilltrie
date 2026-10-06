/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { type Dispatch, type SetStateAction } from "react";

export type ViewType = "domains" | "subjects" | "cosmetics";
export const NavigationButtons = (props: {
    selectedView: ViewType;
    setSelectedView: Dispatch<SetStateAction<ViewType>>;
}) => {
    const SelectButton = ({ name }: { name: ViewType }) => {
        return (
            <button
                onClick={() => props.setSelectedView(name)}
                className={`mockup-btn-style ${props.selectedView === name && "selected"}`}
            >
                {name}
            </button>
        );
    };
    return (
        <div className="flex mt-4 flex-row justify-center">
            <div className="flex flex-row gap-x-6">
                <SelectButton name={"subjects"} />
                <SelectButton name={"domains"} />
            </div>
        </div>
    );
};
