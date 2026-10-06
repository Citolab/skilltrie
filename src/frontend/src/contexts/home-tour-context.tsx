/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { createContext, useContext, useState } from "react";

const HomeTourContext = createContext<{
    tourActive: boolean;

    setTourActive: (active: boolean) => void;
}>({ tourActive: false, setTourActive: () => {} });

export function HomeTourContextProvider({ children }: { children: React.ReactNode }) {
    const [tourActive, setTourActive] = useState(false);

    return (
        <HomeTourContext.Provider value={{ tourActive, setTourActive }}>
            {children}
        </HomeTourContext.Provider>
    );
}

export const useHomeTourContext = () => {
    const context = useContext(HomeTourContext);

    if (!context) {
        throw new Error("useHomeTourContext must be used within a HomeTourContextProvider");
    }

    return context;
};
