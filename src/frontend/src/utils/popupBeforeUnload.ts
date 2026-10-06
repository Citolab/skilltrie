/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { useEffect } from "react";

export function popupBeforeUnload() {
    return useEffect(() => {
        const beforeUnload = (e: any) => {
            e.preventDefault();
        };

        window.addEventListener("beforeunload", beforeUnload);

        return () => {
            window.removeEventListener("beforeunload", beforeUnload);
        };
    }, []);
}
