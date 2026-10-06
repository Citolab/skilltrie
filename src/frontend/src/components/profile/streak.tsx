/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { ProfileWidget } from "../wrappers/profile-widget";
import StreakIcon from "../../assets/streak.svg?react";
import digit0 from "../../assets/numbers/0.svg";
import digit1 from "../../assets/numbers/1.svg";
import digit2 from "../../assets/numbers/2.svg";
import digit3 from "../../assets/numbers/3.svg";
import digit4 from "../../assets/numbers/4.svg";
import digit5 from "../../assets/numbers/5.svg";
import digit6 from "../../assets/numbers/6.svg";
import digit7 from "../../assets/numbers/7.svg";
import digit8 from "../../assets/numbers/8.svg";
import digit9 from "../../assets/numbers/9.svg";

const DIGIT_SRC = [digit0, digit1, digit2, digit3, digit4, digit5, digit6, digit7, digit8, digit9];

export interface StreakProps {
    type: "Current" | "Best";
    streak: number;
}

// The streak number is drawn on the train's signboard (id="board" in streak.svg)
// using the artist-made digit assets. The flame (id="fire") overlaps the board's
// left side up to x ≈ 475, so the digits live in the clear area to the right of
// it. All coordinates below are in the train SVG's own viewBox units. The digits
// are rendered as <image> (each its own isolated document) so the digit assets'
// generic ".cls-N" styles can't clash with the train's.
const SVG_VIEWBOX = "0 0 842.39 385.97";
const BOARD_CENTER_X = 570;
const BOARD_CENTER_Y = 52;
const BOARD_MAX_WIDTH = 170;
const BOARD_MAX_HEIGHT = 80;
// Each digit asset is a fixed-size (monospaced) cell; this is its width / height.
const DIGIT_ASPECT = 423.28 / 396.73;
// Advance per digit as a fraction of a cell's width (< 1 overlaps the cells'
// built-in side padding so the glyphs sit closer together).
const DIGIT_ADVANCE = 0.62;

export function StreakContainer({ type, streak }: StreakProps) {
    const chars = String(Math.max(0, Math.trunc(streak)));
    const digitCount = chars.length;

    // Size the digits to the largest that fits the board in both width and height,
    // so any number of digits always stays inside the board.
    const span = (digitCount - 1) * DIGIT_ADVANCE + 1; // total row width, in cell widths
    const height = Math.min(BOARD_MAX_HEIGHT, BOARD_MAX_WIDTH / span / DIGIT_ASPECT);
    const width = height * DIGIT_ASPECT;
    const advance = width * DIGIT_ADVANCE;
    const startX = BOARD_CENTER_X - (width * span) / 2;
    const topY = BOARD_CENTER_Y - height / 2;

    return (
        <ProfileWidget className="flex-col items-center gap-3 px-8 py-7 w-full">
            <p className="font-title text-text-muted text-lg">{type} streak</p>
            <div className="relative w-64 max-w-full">
                <StreakIcon className="w-full h-auto" />
                <svg className="absolute inset-0 w-full h-full" viewBox={SVG_VIEWBOX} aria-hidden>
                    {[...chars].map((char, i) => (
                        <image
                            key={i}
                            href={DIGIT_SRC[Number(char)]}
                            x={startX + i * advance}
                            y={topY}
                            width={width}
                            height={height}
                            preserveAspectRatio="xMidYMid meet"
                        />
                    ))}
                </svg>
            </div>
        </ProfileWidget>
    );
}
