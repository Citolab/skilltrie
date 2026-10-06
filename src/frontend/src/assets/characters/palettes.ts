/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

/**
 * Four color roles applied to every character:
 *   primaryColor   – main body color
 *   secondaryColor – secondary body parts (ears, belly, snout)
 *   tertiaryColor  – structural details (legs)
 *   accentColor    – facial features (eyes, nose, mouth, sunglasses); usually dark
 */
export interface CharacterPalette {
    primaryColor: string;
    secondaryColor: string;
    tertiaryColor: string;
    accentColor: string;
}

export const CHARACTER_PALETTES = {
    classic: {
        primaryColor: "",
        secondaryColor: "",
        tertiaryColor: "",
        accentColor: "",
    }, // fallback palette with no overrides (all colors default to the SVG's original colors)
    ocean: {
        primaryColor: "#21adcb",
        secondaryColor: "#7fcad2",
        tertiaryColor: "#0a5e70",
        accentColor: "#1d1d1b",
    },
    forest: {
        primaryColor: "#2d6a4f",
        secondaryColor: "#74c69d",
        tertiaryColor: "#1a3d2b",
        accentColor: "#1d1d1b",
    },
    sunset: {
        primaryColor: "#e76f51",
        secondaryColor: "#f4a261",
        tertiaryColor: "#9b3d27",
        accentColor: "#1d1d1b",
    },
    candy: {
        primaryColor: "#f46ba0",
        secondaryColor: "#f8bbd0",
        tertiaryColor: "#de87c2",
        accentColor: "#1d1d1b",
    },
    peach: {
        primaryColor: "#eab1a2",
        secondaryColor: "#f2dccd",
        tertiaryColor: "#e8a769",
        accentColor: "#1d1d1b",
    },
    blossom: {
        primaryColor: "#ffe199",
        secondaryColor: "#fffac2",
        tertiaryColor: "#e9d0e5",
        accentColor: "#1d1d1b",
    },
    sky: {
        primaryColor: "#c9c9e3",
        secondaryColor: "#fef2fe",
        tertiaryColor: "#b7ecf0",
        accentColor: "#1d1d1b",
    },
} satisfies Record<string, CharacterPalette>;

export type PaletteName = keyof typeof CHARACTER_PALETTES;
