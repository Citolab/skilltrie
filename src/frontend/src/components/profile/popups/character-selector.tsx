/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import {
    type Character,
    type UserCharacter,
    defaultUserCharacter,
    type ResolvedCharacter,
    ResolveCharacter,
} from "../../../types/character";
import { Box, FormControl, Radio, RadioGroup, FormControlLabel } from "@mui/material";
import { useState, useEffect, useReducer } from "react";
import { GetUserCharacters, GetCharacters, UpdateCharacter } from "../../../api/character";
import CharacterDisplay, {
    type CharacterType,
} from "../../customisation/characters/character-display";
import { CHARACTER_PALETTES, type PaletteName } from "../../../assets/characters/palettes.ts";
import { PopupLayout } from "../../../components/general-components/popup-layout.tsx";
import { toast } from "react-toastify";
import { useBreakPoints } from "@/hooks/useBreakPoints.tsx";
import { CycleDisplay } from "../../../components/general-components/cycle-display.tsx";
import { PaletteDisplay } from "../../../components/customisation/palette-display.tsx";
import QuestionMarkIcon from "@mui/icons-material/QuestionMark";

interface CharacterSelectorProps {
    open: boolean;
    onClose: () => void;
    onSelect: (char: ResolvedCharacter) => void;
}

type State = {
    userChars: UserCharacter[];
    characters: Character[];
    selectedCharacter: UserCharacter;
};

type Action =
    | { type: "LOAD"; userChars: UserCharacter[]; characters: Character[] }
    | { type: "SELECT_CHARACTER"; id: number }
    | { type: "SELECT_PALETTE"; palette: PaletteName };

/**
 * Reducer function to update the state based on an action
 * @param state The state that should be changed
 * @param action The action type accompanied by the data to update
 * @returns The new state with the update data
 */
function stateReducer(state: State, action: Action): State {
    switch (action.type) {
        case "LOAD": {
            const selected = action.userChars.find((c) => c.selected);
            return {
                ...state,
                userChars: action.userChars,
                characters: action.characters,
                selectedCharacter: selected ? selected : defaultUserCharacter,
            };
        }
        case "SELECT_CHARACTER": {
            const character = state.characters.find((c) => c.id === action.id);
            return {
                ...state,
                selectedCharacter: {
                    ...state.selectedCharacter,
                    id: action.id,
                    name: character!.name,
                },
            };
        }
        case "SELECT_PALETTE":
            return {
                ...state,
                selectedCharacter: {
                    ...state.selectedCharacter,
                    palette: action.palette,
                },
            };
        default:
            return state;
    }
}

export function CharacterRadioButton({
    char,
    state,
    unlocked,
    isMobile,
}: {
    char: Character;
    state: State;
    unlocked: boolean;
    isMobile: boolean;
}) {
    return (
        <Box
            sx={{
                border:
                    state.selectedCharacter.id === char.id
                        ? "3px solid var(--color-borderActive)"
                        : "3px solid transparent",
                bgcolor: "#e6e6e6",
                borderRadius: "5px",
                boxShadow: 1,
                "&:hover": {
                    bgcolor: "#f0f0f0",
                },
            }}
        >
            <div className="relative">
                <FormControlLabel
                    value={char.id}
                    control={<Radio sx={{ display: "none" }} />}
                    label={
                        <CharacterDisplay
                            character={char.name as CharacterType}
                            palette={"classic"}
                            className={`${isMobile ? "w-12 h-14" : "w-16 h-20"}`}
                        />
                    }
                    sx={{ margin: 0, filter: `${unlocked ? "" : "brightness(0) blur(1px)"}` }}
                    className="p-2"
                    disabled={!unlocked}
                />
                {!unlocked && (
                    <QuestionMarkIcon
                        sx={{ scale: 2 }}
                        className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 text-white"
                    />
                )}
            </div>
        </Box>
    );
}

export function CharacterSelector({ open, onClose, onSelect }: CharacterSelectorProps) {
    const [state, dispatch] = useReducer(stateReducer, {
        userChars: [],
        characters: [],
        selectedCharacter: defaultUserCharacter,
    });
    const [error, setError] = useState<boolean>(false);
    const { isMobile } = useBreakPoints();

    useEffect(() => {
        setError(false);

        Promise.all([GetUserCharacters(), GetCharacters()])
            .then(([userChars, characters]) => {
                dispatch({ type: "LOAD", userChars, characters });
            })
            .catch(() => setError(true));
    }, [open]);

    return (
        <PopupLayout
            open={open}
            onClose={onClose}
            title="Select Character"
            headerClassName="primary"
            primaryAction={{
                label: "Save Changes",
                onClick: () => {
                    if (error) return;

                    UpdateCharacter(state.selectedCharacter)
                        .then(() => {
                            onSelect(ResolveCharacter(state.selectedCharacter));
                            onClose();
                            toast.success("Successfully updated character");
                        })
                        .catch(() => toast.error("Something went wrong when saving changes"));
                },
            }}
            secondaryAction={{ label: "Cancel", onClick: onClose }}
            dynamicSizing={true}
        >
            <div>
                {error ? (
                    <p>
                        Something went wrong while loading the character selector. Please try again
                        later.
                    </p>
                ) : (
                    <div
                        className={`flex ${isMobile ? "flex-col" : "flex-row"} justify-around gap-5 mx-2`}
                    >
                        {/* Preview + palette selection part of the selector */}
                        <div className="flex flex-col gap-4 shadow-sm rounded-xl pb-2 px-5">
                            <p className="text-center font-bold text-xl">Preview</p>
                            <CharacterDisplay
                                character={state.selectedCharacter.name as CharacterType}
                                palette={state.selectedCharacter.palette as PaletteName}
                                className={`${isMobile ? "w-30 h-40" : "w-50 h-60"} self-center`}
                            />
                            <div className="self-center mt-auto">
                                <p className="text-center text-gray-600 pb-1">Select Theme</p>
                                <CycleDisplay
                                    options={Object.keys(CHARACTER_PALETTES) as PaletteName[]}
                                    value={state.selectedCharacter.palette as PaletteName}
                                    onChange={(value: PaletteName) =>
                                        dispatch({
                                            type: "SELECT_PALETTE",
                                            palette: value,
                                        })
                                    }
                                    renderItem={(palette) => <PaletteDisplay palette={palette} />}
                                />
                            </div>
                        </div>
                        {/* Character selection part of the selector */}
                        <div className="px-5 py-1 gap-5 flex justify-around">
                            <FormControl>
                                <RadioGroup
                                    value={state.selectedCharacter.id}
                                    onChange={(e) =>
                                        dispatch({
                                            type: "SELECT_CHARACTER",
                                            id: Number(e.target.value),
                                        })
                                    }
                                    sx={{ display: "grid", gridTemplateColumns: "auto auto auto" }}
                                    className="gap-3"
                                >
                                    {state.characters.map((char) => {
                                        const unlocked = state.userChars.some(
                                            (uc) => char.id === uc.id
                                        );
                                        return (
                                            <CharacterRadioButton
                                                key={char.name}
                                                char={char}
                                                state={state}
                                                unlocked={unlocked}
                                                isMobile={isMobile}
                                            />
                                        );
                                    })}
                                </RadioGroup>
                            </FormControl>
                        </div>
                    </div>
                )}
            </div>
        </PopupLayout>
    );
}
