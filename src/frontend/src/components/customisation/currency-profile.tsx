/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import type { Currency, UserCurrency } from "../../types/currency";

export interface CurrencyProfileProps {
    currencies: Currency[];
    userCurrencies: UserCurrency[];
}
export default function CurrencyProfile({
    currencies: curs,
    userCurrencies: userCurs,
}: CurrencyProfileProps) {
    return (
        <div className="flex items-center gap-1 h-full">
            <span className="font-semibold leading-none text-white">Balance:</span>
            {Object.entries(userCurs).map(([key, value]) => {
                const sprite = curs.find((x) => x.id === value.currencyId)?.sprite;
                return (
                    <div key={key} className="flex items-center h-full">
                        <span className="leading-none text-gray-200">{value.amount}</span>
                        <img src={sprite} className="h-10 w-auto pr-4"></img>
                    </div>
                );
            })}
        </div>
    );
}
