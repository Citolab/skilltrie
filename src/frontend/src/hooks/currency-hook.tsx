/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { useState, useEffect } from "react";
import { type Currency, type UserCurrency } from "../types/currency";
import { GetCurrencies, GetUserCurrencies } from "../api/currency";

export function useCurrencies() {
    const [curs, setCurs] = useState<Currency[]>([]);

    useEffect(() => {
        async function load() {
            const data = await GetCurrencies();
            setCurs(data);
        }

        void load();
    }, []);

    return { curs: curs };
}

export function useUserCurrencies() {
    const [uCur, setUCur] = useState<UserCurrency[]>([]);

    async function load() {
        const data = await GetUserCurrencies();
        setUCur(data);
    }

    useEffect(() => {
        void load();
    }, []);

    return { userCurs: uCur, refreshUserCurrencies: load };
}
