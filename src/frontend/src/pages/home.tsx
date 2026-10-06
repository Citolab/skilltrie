/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import React, { useEffect, useState, useRef } from "react";
import "./style/global.css";
import { type ViewType } from "../components/home/navigation-buttons.tsx";
import Tree from "../components/tree/tree.tsx";
import { startHomeTourIfUnseen } from "../components/tours/home-tour.ts";
import CheckNewBadge from "../utils/checkNewBadge.ts";
import NewBadgePopup from "../components/home/newBadge-popup.tsx";
import { type UserBadge } from "../types/badge.ts";
import { HomeTourContextProvider, useHomeTourContext } from "../contexts/home-tour-context.tsx";

export function Home() {
    return (
        <HomeTourContextProvider>
            <HomeContent />
        </HomeTourContextProvider>
    );
}

function HomeContent() {
    const [selectedView, setSelectedView] = useState<ViewType>("domains");
    const [newBadges, setNewBadges] = useState<UserBadge[] | null>(null);
    const hasCheckedBadges = useRef(false);

    const { setTourActive } = useHomeTourContext();

    useEffect(() => {
        document.body.style.overflow = "hidden";
        void startHomeTourIfUnseen(setTourActive);

        if (!hasCheckedBadges.current) {
            hasCheckedBadges.current = true;
            void CheckNewBadge().then((newBadges) => setNewBadges(newBadges));
        }

        return () => {
            document.body.style.overflow = "unset"; // restore when leaving the page
        };
    }, []);

    const SomeComponent = () => {
        const [x, setX] = useState<number>(1);
        // const [x, setX] = [1, (_: any) => {}];
        return (
            <div>
                <button onClick={() => setX((x: any) => x + 1)}>click me</button>
                <p>{x}</p>
            </div>
        );
    };

    const views: Record<string, React.JSX.Element> = {
        domains: <Tree />,
        subjects: <SomeComponent />,
    };

    return (
        <div className="flex flex-col h-full">
            <div className="border-black border-3 w-full h-full">
                {views[selectedView]}
                <section className="border-0 select-none">
                    <NewBadgePopup badges={newBadges} onClose={() => setNewBadges(null)} />
                </section>
            </div>
        </div>
    );
}
