/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import Dog, { type DogProps } from "./dog";
import Frog, { type FrogProps } from "./frog";
import Cat, { type CatProps } from "./cat";
import Lion, { type LionProps } from "./lion";
import Doggo, { type DoggoProps } from "./doggo";
import Owl, { type OwlProps } from "./owl";
import Pigeon, { type PigeonProps } from "./pigeon";
import Penguin, { type PenguinProps } from "./penguin";
import type { BaseCharacterProps } from "./character-base";

export type CharacterType = "dog" | "frog" | "cat" | "lion" | "doggo" | "owl" | "pigeon" | "penguin";

/**
 * Renders any character with optional palette and color overrides.
 *
 * — Basic usage with a preset palette:
 * ```tsx
 * <CharacterDisplay character="owl" palette="forest" className="w-48" />
 * ```
 *
 * — Switching character at runtime (e.g. based on user selection):
 * ```tsx
 * const [character, setCharacter] = useState<CharacterType>("dog");
 * <CharacterDisplay character={character} palette="ocean" className="w-48" />
 * ```
 *
 * — Overriding individual palette colors (applies to all characters):
 * ```tsx
 * <CharacterDisplay character="frog" palette="candy" primaryColor="#ff0000" />
 * ```
 * Individual color props always win over the palette value for that slot.
 *
 * — Character-specific part overrides via *Props (only applied to that character):
 * ```tsx
 * <CharacterDisplay
 *     character="owl"
 *     palette="forest"
 *     owlProps={{ beakColor: "#ffd900", legsColor: "#ffd900" }}
 * />
 * ```
 * Each character exposes its own *Props (dogProps, frogProps, owlProps, …).
 * See the individual character files for which parts are overridable.
 *
 * Available palettes: ocean | forest | sunset | candy | peach | blossom | sky | duolingo
 * Available characters: dog | frog | cat | lion | doggo | owl | pigeon | penguin
 */

interface CharacterDisplayProps extends BaseCharacterProps {
    character: CharacterType;
    dogProps?: Omit<DogProps, keyof BaseCharacterProps>;
    frogProps?: Omit<FrogProps, keyof BaseCharacterProps>;
    catProps?: Omit<CatProps, keyof BaseCharacterProps>;
    lionProps?: Omit<LionProps, keyof BaseCharacterProps>;
    doggoProps?: Omit<DoggoProps, keyof BaseCharacterProps>;
    owlProps?: Omit<OwlProps, keyof BaseCharacterProps>;
    pigeonProps?: Omit<PigeonProps, keyof BaseCharacterProps>;
    penguinProps?: Omit<PenguinProps, keyof BaseCharacterProps>;
}

export default function CharacterDisplay({
    character,
    dogProps, frogProps, catProps, lionProps,
    doggoProps, owlProps, pigeonProps, penguinProps,
    ...baseProps
}: CharacterDisplayProps) {
    if (character === "dog")     return <Dog     {...baseProps} {...dogProps} />;
    if (character === "frog")    return <Frog    {...baseProps} {...frogProps} />;
    if (character === "cat")     return <Cat     {...baseProps} {...catProps} />;
    if (character === "lion")    return <Lion    {...baseProps} {...lionProps} />;
    if (character === "doggo")   return <Doggo   {...baseProps} {...doggoProps} />;
    if (character === "owl")     return <Owl     {...baseProps} {...owlProps} />;
    if (character === "pigeon")  return <Pigeon  {...baseProps} {...pigeonProps} />;
    if (character === "penguin") return <Penguin {...baseProps} {...penguinProps} />;
}