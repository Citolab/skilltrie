/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import background from "../../assets/background.svg";
import { ProfileWidget } from "../wrappers/profile-widget";
import CharacterDisplay, {
    type CharacterType,
} from "../customisation/characters/character-display";
import type { PaletteName } from "../../assets/characters/palettes";
import { CharacterSelector } from "./popups/character-selector";
import { useEffect, useState } from "react";
import type { ResolvedCharacter } from "@/types/character";
import { IconButton } from "@mui/material";
import { PencilIcon } from "@heroicons/react/24/outline";
import { GetActiveUser } from "@/api/user";
import { type PublicUser, defaultPublicUser } from "@/types/user";
import { useBreakPoints } from "@/hooks/useBreakPoints";

/** Intrinsic size of the station scene SVG (its viewBox, in user units). */
const SCENE_W = 1924.11;
const SCENE_H = 1089.06;

/**
 * The cut-out window into the scene that the widget shows — a zoomed-in shot
 * framing the character between the platform rails and the station building.
 * Coordinates are in scene units; tweak these numbers to re-frame the shot.
 *
 * On wide layouts we show a broad panoramic strip; on mobile the widget is
 * narrow, so the same wide crop would shrink the character to a dot. The mobile
 * crop keeps the same vertical framing but zooms in horizontally, centred on the
 * character (scene centre x ≈ 962), so it stays large on small screens.
 */
const CROP_DESKTOP = { x: 200, y: 200, w: 1550, h: 540 };
const CROP_MOBILE = { x: 564, y: 200, w: 796, h: 540 };

export interface CharacterContainerProps {
    character: CharacterType;
    palette: PaletteName;
    onSelect: (char: ResolvedCharacter) => void;
    displayedUser: PublicUser;
}

export function CharacterContainer({
    character,
    palette,
    onSelect,
    displayedUser,
}: CharacterContainerProps) {
    const [user, setUser] = useState<PublicUser>(defaultPublicUser);

    useEffect(() => {
        GetActiveUser()
            .then(setUser)
            .catch((error) => {
                console.error(error);
            });
    }, []);

    const [selectCharacter, setSelectCharacter] = useState<boolean>(false);

    const { isMobile } = useBreakPoints();
    const CROP = isMobile ? CROP_MOBILE : CROP_DESKTOP;

    return (
        <ProfileWidget
            className="rounded-4xl relative overflow-hidden"
            style={{ aspectRatio: `${CROP.w} / ${CROP.h}` }}
        >
            {/* The full station scene, scaled up and offset so that the CROP window
                exactly fills the widget — i.e. a zoomed-in cut-out of the scene. The
                character lives inside this layer, so its placement stays relative to the
                whole scene (between the tracks and the station building). */}
            <div
                className="absolute bg-cover bg-center"
                style={{
                    width: `${(SCENE_W / CROP.w) * 100}%`,
                    height: `${(SCENE_H / CROP.h) * 100}%`,
                    left: `${(-CROP.x / CROP.w) * 100}%`,
                    top: `${(-CROP.y / CROP.h) * 100}%`,
                    backgroundImage: `url(${background})`,
                }}
            >
                <CharacterDisplay
                    character={character}
                    palette={palette}
                    className="absolute left-1/2 bottom-[40%] -translate-x-1/2 h-[30%] [&>svg]:h-full [&>svg]:w-auto"
                />
            </div>
            <CharacterSelector
                open={selectCharacter}
                onClose={() => setSelectCharacter(false)}
                onSelect={onSelect}
            />
            {displayedUser.id === user.id && (
                <IconButton
                    onClick={() => setSelectCharacter(true)}
                    sx={{
                        position: "absolute",
                        top: 12,
                        right: 12,
                        color: "var(--color-gray-700)",
                        backgroundColor: "color-mix(in oklab, var(--color-white) 70%, transparent)",
                        boxShadow: 2,
                        border: "1px solid var(--color-gray-300)",
                        transition: "all 150ms ease",
                        "&:hover": {
                            color: "var(--color-primary)",
                            backgroundColor: "var(--color-white)",
                            transform: "scale(1.05)",
                        },
                    }}
                    aria-label="Edit user character"
                >
                    <PencilIcon className="w-8" />
                </IconButton>
            )}
        </ProfileWidget>
    );
}
