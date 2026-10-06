/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { useState, useEffect } from "react";
import { type Cosmetic, type UserCosmetic } from "../../types/cosmetic";
import {
    GetUserCosmetics,
    GetCosmetics,
    EquipUserCosmetic,
    BuyUserCosmetic,
    SellUserCosmetic,
} from "../../api/cosmetic";

export function useCosmetics() {
    const [cosms, setCosms] = useState<Cosmetic[]>([]);
    useEffect(() => {
        async function load() {
            const cosData: Cosmetic[] = await GetCosmetics();
            setCosms(cosData);
        }
        void load();
    }, []);

    return { allCosmetics: cosms };
}

export function useUserCosmetics(refreshUserCurrencies: () => Promise<void>) {
    const [userCosms, setUserCosms] = useState<UserCosmetic[]>([]);

    async function load() {
        const ucosData: UserCosmetic[] = await GetUserCosmetics();
        setUserCosms(ucosData);
    }

    async function equipCosmetic(ucos: UserCosmetic) {
        try {
            await EquipUserCosmetic(ucos);
            await load();
        } catch (error) {
            console.error("Failed to update cosmetic", error);
            throw error;
        }
    }
    async function buyCosmetic(ucos: UserCosmetic) {
        try {
            await BuyUserCosmetic(ucos);
            await load();
            await refreshUserCurrencies();
        } catch (error) {
            console.error("Failed to buy cosmetic", error);
            throw error;
        }
    }

    async function sellCosmetic(ucos: UserCosmetic) {
        try {
            await SellUserCosmetic(ucos);
            await load();
            await refreshUserCurrencies();
        } catch (error) {
            console.error("Failed to sell cosmetic", error);
            throw error;
        }
    }
    useEffect(() => {
        void load();
    }, []);

    return {
        userCosmetics: userCosms,
        equipUserCos: equipCosmetic,
        buyUserCosmetic: buyCosmetic,
        sellUserCosmetic: sellCosmetic,
    };
}
