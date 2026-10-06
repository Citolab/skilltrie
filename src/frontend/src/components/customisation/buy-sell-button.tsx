/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import type { Cosmetic } from "../../types/cosmetic";

export interface BuySellButtonProps {
    cosmetic: Cosmetic;
    bought: boolean;
    onClickBuy: (cosmeticId: number) => void;
    onClickSell: (cosmeticId: number) => void;
}

export default function BuySellButton({
    cosmetic,
    bought,
    onClickBuy,
    onClickSell,
}: BuySellButtonProps) {
    return (
        <div className="flex flex-col h-40 w-40 border rounded-2xl relative">
            <img className="flex w-full h-32" src={cosmetic.iconFile}></img>
            <span className="absolute top-1 left-1 bg-yellow-500 text-black text-xs px-2 rounded">
                {cosmetic.price}
            </span>
            {bought && (
                <span className="absolute top-1 right-1 bg-green-600 text-white text-xs px-2 rounded">
                    Owned
                </span>
            )}
            <div className="flex h-8 w-full ">
                <button
                    className="flex-1 cursor-pointer text-center bg-green-400 rounded-bl-2xl"
                    onClick={() => onClickBuy(cosmetic.id)}
                    disabled={bought}
                >
                    Buy
                </button>
                <button
                    className="flex-1 cursor-pointer text-center bg-red-400 rounded-br-2xl"
                    onClick={() => onClickSell(cosmetic.id)}
                    disabled={!bought}
                >
                    Sell
                </button>
            </div>
        </div>
    );
}
