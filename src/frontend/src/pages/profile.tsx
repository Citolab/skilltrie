/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { useNavigate, useParams } from "react-router-dom";
import GeneralButton from "../components/general-components/general-button.tsx";
import { StreakContainer } from "../components/profile/streak.tsx";
import { BadgesContainer, Passport } from "../components/profile/badges.tsx";
import { CharacterContainer } from "../components/profile/character.tsx";
import { useEffect, useState } from "react";
import { type PublicUser, defaultPublicUser } from "../types/user.ts";
import { GetPublicUserByName } from "../api/user.ts";
import { GetUserStreak } from "../api/streak.ts";
import { GetBadgesByUserId } from "../api/badge.ts";
import { GetSelectedUserCharacter } from "../api/character.ts";
import type { UserStreak } from "../types/streak.ts";
import type { UserBadge } from "../types/badge.ts";
import { ResolveCharacter, type ResolvedCharacter } from "../types/character.ts";
import { ProfileWidget } from "../components/wrappers/profile-widget.tsx";
import { ArrowLeftIcon } from "@heroicons/react/24/outline";
import type { ServerError } from "../types/error.ts";
import { NotFoundPage } from "./not-found.tsx";

// A user that has not completed anything yet has no streak record (the endpoint
// returns 404). That is a normal state, not a failure, so we fall back to zeroes.
function fetchStreak(userId: number): Promise<UserStreak> {
    return GetUserStreak(userId).catch((err: ServerError) => {
        if (err.response?.status === 404) {
            return { userId, lastDayCompleted: "", currentStreak: 0, highestStreak: 0 };
        }
        throw err;
    });
}

export function Profile() {
    const navigate = useNavigate();

    const { userName } = useParams();
    const [user, setUser] = useState<PublicUser>(defaultPublicUser);
    const [notFound, setNotFound] = useState<boolean>(false);

    const [streak, setStreak] = useState<UserStreak | null>(null);
    const [badges, setBadges] = useState<UserBadge[]>([]);
    const [character, setCharacter] = useState<ResolvedCharacter | null>(null);
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<boolean>(false);

    useEffect(() => {
        if (!userName) return;

        GetPublicUserByName(String(userName))
            .then((user: PublicUser) => setUser(user))
            .catch((err: ServerError) => {
                if (err.response.status === 404) setNotFound(true);
            });
    }, [userName]);

    useEffect(() => {
        if (!user.id) return;

        setLoading(true);
        setError(false);

        Promise.all([
            fetchStreak(user.id),
            GetBadgesByUserId(user.id),
            GetSelectedUserCharacter(user.id),
        ])
            .then(([streakData, badgeData, characterData]) => {
                setStreak(streakData);
                setBadges(badgeData);
                setCharacter(ResolveCharacter(characterData));
            })
            .catch(() => setError(true))
            .finally(() => setLoading(false));
    }, [user.id]);

    if (notFound) return <NotFoundPage />;
    return (
        <div className="flex flex-col justify-center p-2 mx-5">
            <div className="relative items-center mb-6">
                <GeneralButton
                    type="button"
                    variant="ghost"
                    size="medium"
                    onClick={() => navigate(-1)}
                    positionStyling={{ position: "absolute", left: -20, top: 0, zIndex: 10 }}
                >
                    <ArrowLeftIcon className="w-6 h-6" />
                </GeneralButton>
                <h1 className="titleText text-center self-center">Profile</h1>
            </div>

            <div className={`flex flex-col self-center gap-3 w-full sm:w-1/2`}>
                {loading ? (
                    <p className="basicText text-center py-4">Loading…</p>
                ) : error || !character ? (
                    <p className="basicText text-center py-4 text-red-500">
                        Something went wrong while loading this profile. Please try again later.
                    </p>
                ) : (
                    <>
                        <CharacterContainer
                            character={character.character}
                            palette={character.palette}
                            onSelect={setCharacter}
                            displayedUser={user}
                        />

                        {/* Username + fullname part of the profile page */}
                        <ProfileWidget className="flex-col p-3 basicText">
                            <p className="font-bold text-2xl">{user.displayName}</p>
                        </ProfileWidget>

                        <div className="flex flex-wrap gap-5">
                            <div className="flex-1 min-w-[18rem]">
                                <StreakContainer type="Current" streak={streak?.currentStreak ?? 0} />
                            </div>
                            <div className="flex-1 min-w-[18rem]">
                                <StreakContainer type="Best" streak={streak?.highestStreak ?? 0} />
                            </div>
                        </div>
                        <Passport userBadges={badges} />
                        <BadgesContainer userBadges={badges} />
                    </>
                )}
            </div>
        </div>
    );
}
