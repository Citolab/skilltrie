/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { LockClosedIcon } from "@heroicons/react/24/solid";
import { ChevronLeftIcon, ChevronRightIcon } from "@heroicons/react/24/outline";
import { ProfileWidget } from "../wrappers/profile-widget";
import BadgeDetails from "./popups/badge-popup.tsx";
import { parseStampImage } from "../../utils/stamp-options.ts";
import type { UserBadge, UserStamp } from "../../types/badge.ts";
import { useEffect, useState } from "react";
import passportSvg from "../../assets/passport.svg";

interface BadgeProps {
    image: string;
    name: string;
    requirement?: string;
    unlocked: boolean;
    unlockDate?: string;
    progress: number;
    progressNeeded: number;
}

export function Badge(props: BadgeProps) {
    const dateDisplay = props.unlockDate ? new Date(props.unlockDate).toLocaleDateString() : "";
    return (
        <ProfileWidget className="bg-background-light flex flex-col items-center p-3 w-32 relative font-ui hover:-translate-y-1 hover:scale-103 transition duration-150 ease-in-out">
            {!props.unlocked && <LockClosedIcon className="w-12 absolute top-0 -right-1" />}
            <img
                src={props.image}
                alt="Achievement image"
                className={`w-16 ${!props.unlocked && "grayscale"}`}
            />
            <div className="flex flex-col justify-between flex-1 w-full">
                <p className="line-clamp-2 text-center text-sm whitespace-normal break-words">{props.name}</p>
                <p className="text-xs text-center p-2">
                    {props.unlocked ? (
                        <span className="text-yellow-500 whitespace-normal break-words w-full text-center">accomplished at {dateDisplay}</span>
                    ) : (
                        <span className="text-text-muted ">locked</span>
                    )}
                </p>
            </div>
        </ProfileWidget>
    );
}

export function mapBadge(badge: UserBadge): BadgeProps {
    return {
        image: parseStampImage(badge.stampImage),
        name: badge.name ?? "Unknown Badge",
        requirement: badge.description,
        unlockDate: badge.stamp?.dateAccomplished,
        unlocked: badge.stamp != null,
        progress: badge.progress ?? 0,
        progressNeeded: badge.progressNeeded,
    };
}

const GRID_W = 4;
const GRID_H = 8;
const STAMP_W = 2;
const STAMP_H = 2;

function badgeStyle(stamp: UserStamp): React.CSSProperties {
    return {
        position: "absolute",
        left: `${(stamp.x / GRID_W) * 100}%`,
        top: `${(stamp.y / GRID_H) * 100}%`,
        width: `${(STAMP_W / GRID_W) * 100}%`,
        height: `${(STAMP_H / GRID_H) * 100}%`,
    };
}

export function Passport({ userBadges }: { userBadges: UserBadge[] }) {
    const [activeBadge, setActiveBadge] = useState<BadgeProps | null>(null);
    const [currentPage, setCurrentPage] = useState(1);

    const TOTAL_PAGES = userBadges.reduce(
        (max, b) => (b.stamp ? Math.max(max, b.stamp.page + 1) : max),
        1
    );

    const currentBadges = userBadges.filter((b) => b.stamp && b.stamp.page === currentPage - 1);

    useEffect(() => {
        const handleKeyDown = (e: KeyboardEvent) => {
            if (e.key === "ArrowLeft") setCurrentPage((p) => Math.max(1, p - 1));
            if (e.key === "ArrowRight") setCurrentPage((p) => Math.min(TOTAL_PAGES, p + 1));
        };
        window.addEventListener("keydown", handleKeyDown);
        return () => window.removeEventListener("keydown", handleKeyDown);
    }, [TOTAL_PAGES]);

    return (
        <div>
            <h2 className="sloganText text-3xl text-center m-1">Passport</h2>

            <div className="relative w-full">
                <img src={passportSvg} alt="Passport" className="w-full h-auto" />

                {currentBadges.map((badge, index) => (
                    <div
                        key={index}
                        style={badgeStyle(badge.stamp)}
                        className="cursor-pointer hover:scale-105 transition-transform"
                        onClick={() => setActiveBadge(mapBadge(badge))}
                    >
                        <img
                            src={parseStampImage(badge.stampImage)}
                            alt={badge.name ?? "Badge"}
                            className="w-full h-full object-contain"
                        />
                    </div>
                ))}

                {currentPage > 1 && (
                    <button
                        onClick={() => setCurrentPage((p) => p - 1)}
                        className="absolute -left-10 top-1/2 -translate-y-1/2 p-1"
                    >
                        <ChevronLeftIcon
                            className="w-10 h-10 stroke-[var(--color-primary)]"
                            aria-label="Previous page"
                        />
                    </button>
                )}

                {currentPage < TOTAL_PAGES && (
                    <button
                        onClick={() => setCurrentPage((p) => p + 1)}
                        className="absolute -right-10 top-1/2 -translate-y-1/2 p-1"
                    >
                        <ChevronRightIcon
                            className="w-10 h-10 stroke-[var(--color-primary)]"
                            aria-label="Next page"
                        />
                    </button>
                )}
            </div>

            <p className="text-center text-lg mt-1 text-[var(--color-primary)]">
                {currentPage} / {TOTAL_PAGES}
            </p>

            <BadgeDetails
                open={!!activeBadge}
                badge={activeBadge}
                onClose={() => setActiveBadge(null)}
            />
        </div>
    );
}

export function BadgesContainer({ userBadges }: { userBadges: UserBadge[] }) {
    const [activeBadge, setActiveBadge] = useState<BadgeProps | null>(null);
    const badges = userBadges.map(mapBadge);

    const displayedBadges = badges.filter((b) => !b.unlocked);

    return (
        <div>
            <h2 className="sloganText text-3xl text-center m-1">Achievements to unlock</h2>
            <ProfileWidget className="flex flex-wrap content-start justify-center gap-x-4 gap-y-3 overflow-auto p-3 h-100 w-full">
                {displayedBadges.map((badge, index) => (
                    <div
                        key={index}
                        onClick={() => setActiveBadge(badge)}
                        className="flex h-40 w-32 justify-center"
                    >
                        {Badge(badge)}
                    </div>
                ))}
            </ProfileWidget>

            <BadgeDetails
                open={!!activeBadge}
                badge={activeBadge}
                onClose={() => setActiveBadge(null)}
            />
        </div>
    );
}
