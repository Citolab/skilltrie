/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import type { Cosmetic } from "../../types/cosmetic";

interface EquipButtonProps {
    cosmetic: Cosmetic;
    equipped: boolean;
    onClickEquip: (cosmeticId: number, equipped: boolean) => void;
}

export default function EquipButton({ cosmetic, equipped, onClickEquip }: EquipButtonProps) {
    return (
        <div className=" flex h-40 w-40 ">
            <button
                className={`flex cursor-pointer border-2 rounded-2xl ${equipped ? "border-green-600" : "opacity-80"}`}
                onClick={() => onClickEquip(cosmetic.id, !equipped)}
            >
                <img className=" w-full h-full object-contain " src={cosmetic.iconFile}></img>
            </button>
        </div>
    );
}
