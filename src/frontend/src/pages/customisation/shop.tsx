/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { type Cosmetic, type UserCosmetic } from "../../types/cosmetic";
import BuySellButton from "../../components/customisation/buy-sell-button";

import { useOutletContext } from "react-router-dom";
import { OwnedCosmetics } from "../../utils/customisation/types-to-rive";
export type CustomisationOutletContext = {
    allCosmetics: Cosmetic[];
    userCosmetics: UserCosmetic[];
    buyUserCosmetic: (ucos: UserCosmetic) => Promise<void>;
    sellUserCosmetic: (ucos: UserCosmetic) => Promise<void>;
};

export default function Shop() {
    const { allCosmetics, userCosmetics, buyUserCosmetic, sellUserCosmetic } =
        useOutletContext<CustomisationOutletContext>();

    const ownedCosmetics = OwnedCosmetics(allCosmetics, userCosmetics);

    return (
        <div>
            <div className="grid grid-cols-6 gap-4 p-4">
                {allCosmetics.map((cosmetic: Cosmetic) => {
                    const owned: boolean = ownedCosmetics.some((c) => c === cosmetic);
                    return (
                        <div key={cosmetic.id} className="flex h-full w-full">
                            <BuySellButton
                                cosmetic={cosmetic}
                                bought={owned}
                                onClickBuy={(ucos) => {
                                    void buyUserCosmetic({
                                        cosmeticId: ucos,
                                        equipped: false,
                                    });
                                }}
                                onClickSell={(ucos) => {
                                    void sellUserCosmetic({
                                        cosmeticId: ucos,
                                        equipped: false,
                                    });
                                }}
                            />
                        </div>
                    );
                })}
            </div>
        </div>
    );
}
