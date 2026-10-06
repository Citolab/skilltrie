/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { useEffect, useRef } from "react";
import {
    type ComputedItem,
} from "@citolab/qti-components";

export default function QTIReflectionItem({ item }: { item: ComputedItem }) {
    const qtiItemRef = useRef<HTMLElement & { configContext?: object }>(null);

    useEffect(() => {
        const host = qtiItemRef.current;
        if (!host) return;

        // 1. Configure how the correct answer is shown on this item.
        //    'full' renders a second read-only interaction with the correct answer.
        //    'internal' marks correct/incorrect on the candidate's own interaction.
        host.configContext = {
            correctResponseMode: "internal",
            fullCorrectResponseOnlyWhenIncorrect: true,
        };

        // 2. When the assessment item connects, replay the candidate's variables
        //    and trigger the "show correct response" mode.
        function onConnected(e: Event) {
            e.stopPropagation(); // keep events from bubbling to any outer test player
            const assessmentItem = (e as CustomEvent<any>).detail;
            assessmentItem.variables = item.variables;
            assessmentItem.refresh?.();
            assessmentItem.showCorrectResponse(true);
        };

        host.addEventListener("qti-assessment-item-connected", onConnected);
        return () => host.removeEventListener("qti-assessment-item-connected", onConnected);
    }, [item]);

    return (
        <div style={{ pointerEvents: "none" }}>
            <qti-item ref={qtiItemRef}>
                <item-container item-url={item.href}></item-container>
            </qti-item>
        </div>
    );
}
