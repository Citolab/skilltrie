/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { driver } from "driver.js";
import "driver.js/dist/driver.css";
import { GetTourSeen, MarkTourSeen } from "../../api/tour";

const TOUR_KEY = "home-tour";

function createHomeDriver(setTourActive: (active: boolean) => void) {
    return driver({
        popoverClass: "skilltrie",
        showProgress: true,
        showButtons: ["next", "previous", "close"],
        disableActiveInteraction: true,
        overlayClickBehavior: "nextStep",
        onHighlighted: () => setTourActive(true),
        onDestroyed: () => setTourActive(false),

        steps: [
            {
                popover: {
                    title: "Welcome to Skilltrie",
                    description: "Let's do a quick introduction tour to get you started!",
                    side: "top",
                    align: "start",
                },
            },
            {
                element: ".base-node-border--suggested",
                popover: {
                    title: "Levels",
                    description: "This is a level, each level covers a unique topic.",
                    side: "top",
                    align: "start",
                },
            },
            {
                element: ".node-tooltip",
                popover: {
                    title: "Start a level",
                    description:
                        'After selecting a level, just press "Start Level" to jump right in!',
                    side: "top",
                    align: "start",
                },
            },
            {
                element: ".cursor-pointer",
                popover: {
                    title: "Domains",
                    description:
                        "By completing levels, you can unlock new domains, with new levels to practice. You can press on a domain to get a preview of upcoming levels!",
                    side: "top",
                    align: "start",
                },
            },
            {
                element: ".react-flow__minimap-svg",
                popover: {
                    title: "Minimap",
                    description:
                        "For a quick overview of all domains, you can use the minimap in the corner",
                    side: "top",
                    align: "start",
                },
            },
            {
                element: ".text-text-dark",
                popover: {
                    title: "Closing domains",
                    description:
                        "Or you can close your current domain by pressing the button above.",
                    side: "top",
                    align: "start",
                },
            },
            {
                element: 'a[href*="/profile/"]',
                popover: {
                    title: "Your profile",
                    description:
                        "By completing levels, you can earn badges and increase your streak. Check it out on your profile page!",
                    side: "top",
                    align: "start",
                },
            },
            {
                element: ".MuiButtonBase-root",
                popover: {
                    title: "Information",
                    description:
                        "More information regarding topics and mastery can be found here. You can also restart the tutorial from here.",
                    side: "top",
                    align: "start",
                },
            },
            {
                popover: {
                    title: "Go get 'em, Champ!",
                    description: "You are now ready to start your Skilltrie journey.",
                    side: "top",
                    align: "start",
                },
            },
        ],
    });
}

/**
 * Starts the Home tour automatically if the user hasn't seen it yet.
 * Call this inside a useEffect on the Home page.
 */
export async function startHomeTourIfUnseen(
    setTourActive: (active: boolean) => void
): Promise<void> {
    try {
        const seen = await GetTourSeen(TOUR_KEY);

        if (!seen) {
            const driver = createHomeDriver(setTourActive);
            await MarkTourSeen(TOUR_KEY);
            driver.drive();
        }
    } catch (error) {
        console.error("Failed to start home tour", error);
    }
}

/**
 * Manually starts the Home tour regardless of seen state.
 * Use this in case we want to replay the tutorial later on.
 */
export function startHomeTour(setTourActive: (active: boolean) => void): void {
    try {
        const driver = createHomeDriver(setTourActive);
        driver.drive();
    } catch (error) {
        console.error("Failed to start home tour", error);
    }
}
