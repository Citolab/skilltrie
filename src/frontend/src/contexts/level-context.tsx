/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import {
    createContext,
    type Dispatch,
    type ReactNode,
    type SetStateAction,
    useContext,
    useState,
} from "react";
import type { LevelResponse } from "../types/level.ts";

const LevelContext = createContext<{
    levelResponse: LevelResponse | null;
    setLevelResponse: Dispatch<SetStateAction<LevelResponse | null>>;
}>({ levelResponse: null, setLevelResponse: () => {} });

export function LevelContextProvider({ children }: { children: ReactNode }) {
    let [levelResponse, setLevelResponse] = useState<LevelResponse | null>(null);

    return (
        <LevelContext.Provider value={{ levelResponse, setLevelResponse }}>
            {children}
        </LevelContext.Provider>
    );
}

export const useLevelContext = () => {
    const context = useContext(LevelContext);
    if (!context) {
        throw new Error("useLevelContext must be used within a LevelContextProvider");
    }
    return context;
};
