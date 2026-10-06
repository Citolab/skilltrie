/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import CurrencyProfile, { type CurrencyProfileProps } from "./currency-profile.tsx";
import { NavLink } from "react-router-dom";

export function CustomisationHeader(currencyProps: CurrencyProfileProps) {
    return (
        <div className="flex w-full h-full bg-gray-800">
            <div className="flex-1 flex items-center justify-evenly text-2xl italic">
                <NavLink
                    to="inventory"
                    className={({ isActive }) =>
                        `cursor-pointer hover:text-primary-tint ${isActive ? "border-b border-white" : "text-slate-300"}`
                    }
                >
                    Inventory
                </NavLink>
                <NavLink
                    to="shop"
                    className={({ isActive }) =>
                        `cursor-pointer hover:text-primary-tint ${isActive ? "border-b border-white" : "text-slate-300"}`
                    }
                >
                    Shop
                </NavLink>
            </div>
            <div className="flex ml-auto h-full w-auto">
                <CurrencyProfile {...currencyProps} />
            </div>
        </div>
    );
}
