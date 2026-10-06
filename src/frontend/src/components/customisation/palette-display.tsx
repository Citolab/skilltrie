/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { CHARACTER_PALETTES, type PaletteName } from "../../assets/characters/palettes.ts";

export function PaletteDisplay({ palette }: { palette: PaletteName }) {
    const paletteColours = CHARACTER_PALETTES[palette];
    return (
        <div className="grid grid-cols-2 w-14 h-14 rounded-xl overflow-hidden">
            {Object.entries(paletteColours).map(([key, colour]) => {
                return (
                    <span
                        key={key}
                        style={{
                            backgroundColor: colour,
                        }}
                    />
                );
            })}
        </div>
    );
}
