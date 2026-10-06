/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { driver } from "driver.js"; 
import "driver.js/dist/driver.css"; 
import { GetTourSeen, MarkTourSeen } from "../../api/tour";

const TOUR_KEY = "qti-tour";

function createQtiDriver() {
    return driver({
        popoverClass: "skilltrie",
        showProgress: true,
        showButtons: ["next", "previous", "close"],
        disableActiveInteraction: true,
        overlayClickBehavior: "nextStep",
        steps: [
            { popover: { title: "Welcome to your first level!", description: "We'll show you how it all works!", side: "top", align: "start" }},
            { element: ".leading-relaxed:first-of-type", popover: { title: "The question", description: "Here, you find the current question. Answer by pressing one of the options.", side: "top", align: "start" }},
            { element: ".border-qti-button-yellow-border", popover: { title: "Skip questions", description: "If you don't know the answer, you can skip it and come back later. Once you select an option, this button lets you go to the next question.", side: "top", align: "start" }},
            { element: ".flex.items-center.w-full", popover: { title: "Progress", description: "This progress bar shows you how far you are in the level.", side: "top", align: "start" }},
            { element: ".flex.overflow-x-auto.w-full.min-w-0", popover: { title: "Navigation", description: "Press on a number to go to any question you like.", side: "top", align: "start" }},
            { element: ".border-qti-button-report-border", popover: { title: "Report questions", description: "Something wrong with this specific question? No worries, just report it!", side: "top", align: "start" }},
            { element: ".border-\\[\\#00b1aa\\]", popover: { title: "Finish level", description: "Once you are completely done with this level, you can see your results by finishing the level.", side: "top", align: "start" }},
            { element: ".w-full.aspect-square", popover: { title: "Make Mr. Nose proud!", description: "Do well on a test, and you'll gain his approval!", side: "top", align: "start" }},
        ],
    });
}

/**
 * Starts the QTI tour automatically if the user hasn't seen it yet.
 * Call this inside a useEffect on the QTI page.
 */
export async function startQtiTourIfUnseen(): Promise<void> {
    try {
        const seen = await GetTourSeen(TOUR_KEY);

        if (!seen) {
            const driver = createQtiDriver();
            await MarkTourSeen(TOUR_KEY);
            driver.drive();
        }
    } catch (error) {
        console.error("Failed to start QTI tour", error);
    }
}

/**
 * Manually starts the QTI tour regardless of seen state.
 * Use this in case we want to replay the tutorial later on. 
 */
export function startQtiTour(): void {
    try{
        const driver = createQtiDriver();
        driver.drive();
    } catch (error) {
        console.error("Failed to start QTI tour", error);
    }
}