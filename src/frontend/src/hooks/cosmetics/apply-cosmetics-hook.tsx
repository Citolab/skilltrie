/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { useEffect } from "react";
import { type Cosmetic } from "../../types/cosmetic";

export function useApplyCosmetics(
    equipped: Cosmetic[],
    allCosmetics: Cosmetic[],
    setRiveInput: (key: string, value: number) => void
) {
    useEffect(() => {
        const allInputs = new Set(allCosmetics.map((c) => c.riveInput));
        allInputs.forEach((k) => setRiveInput(k, 0));

        equipped.forEach((c) => setRiveInput(c.riveInput, c.riveInputValue));
    }, [equipped, setRiveInput, allCosmetics]);
}
