/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { type Cosmetic, type UserCosmetic } from "../../types/cosmetic";
import EquipButton from "../../components/customisation/equip-button";

import { useOutletContext } from "react-router-dom";
import { OwnedCosmetics } from "../../utils/customisation/types-to-rive";
export type CustomisationOutletContext = {
    allCosmetics: Cosmetic[];
    userCosmetics: UserCosmetic[];
    equipUserCos: (ucos: UserCosmetic) => Promise<void>;
};
export default function Inventory() {
    const { allCosmetics, userCosmetics, equipUserCos } =
        useOutletContext<CustomisationOutletContext>();

    const ownedCosmetics = OwnedCosmetics(allCosmetics, userCosmetics);

    return (
        <div>
            <div className="grid grid-cols-6 gap-4 p-4">
                {ownedCosmetics.map((cosmetic: Cosmetic) => {
                    const userCos = userCosmetics.find((u) => u.cosmeticId === cosmetic.id);
                    return (
                        <div key={cosmetic.id} className="flex h-full w-full">
                            <EquipButton
                                cosmetic={cosmetic}
                                equipped={userCos!.equipped}
                                onClickEquip={(cosId, eq) =>
                                    void equipUserCos({
                                        userId: userCos!.userId,
                                        cosmeticId: cosId,
                                        equipped: eq,
                                    })
                                }
                            />
                        </div>
                    );
                })}
            </div>
        </div>
    );
}
