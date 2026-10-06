/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { Outlet } from "react-router-dom";
import { CustomisationHeader } from "../../components/customisation/customisation-header";
import Avatar from "../../components/customisation/avatar";

import { useCurrencies, useUserCurrencies } from "../../hooks/currency-hook";
import { useAvatar } from "../../hooks/cosmetics/avatar-rive-hook";
import { useCosmetics, useUserCosmetics } from "../../hooks/cosmetics/cosmetics-hook";
import { EquippedCosmetics } from "../../utils/customisation/types-to-rive";
import { useApplyCosmetics } from "../../hooks/cosmetics/apply-cosmetics-hook";

export default function CustomisationLayout() {
    const { curs } = useCurrencies();
    const { userCurs, refreshUserCurrencies } = useUserCurrencies();
    const { allCosmetics } = useCosmetics();
    const { userCosmetics, equipUserCos, buyUserCosmetic, sellUserCosmetic } =
        useUserCosmetics(refreshUserCurrencies);
    const { riveComponent, setRiveInput } = useAvatar();
    const equipped = EquippedCosmetics(allCosmetics, userCosmetics);

    useApplyCosmetics(equipped, allCosmetics, setRiveInput);

    return (
        <div className="min-h-screen h-screen flex flex-col bg-gray-600 text-gray-200 overflow-hidden">
            <div className="flex-1 grid grid-cols-3">
                <div className="col-span-1">
                    <Avatar RiveComponent={riveComponent} previewText="Your Avatar" />
                </div>
                <div className="col-span-2">
                    <div className="flex-none h-12 w-full">
                        <CustomisationHeader currencies={curs} userCurrencies={userCurs} />
                    </div>
                    <Outlet
                        context={{
                            allCosmetics,
                            userCosmetics,
                            equipUserCos,
                            buyUserCosmetic,
                            sellUserCosmetic,
                        }}
                    />
                </div>
            </div>
        </div>
    );
}
