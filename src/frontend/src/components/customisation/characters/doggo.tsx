/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import DoggoSVG from "@/assets/characters/doggo.svg?react";
import { createAddRule, createCharacter, type BaseCharacterProps, type StyleBuilder } from "./character-base";

/**
 * Per-part color overrides for the doggo character.
 * All props are optional; unset parts fall back to the palette color for their group.
 */
export interface DoggoProps extends BaseCharacterProps {
    bodyColor?: string;
    tailColor?: string;
    earsColor?: string;
    legsColor?: string;
}

/**
 * SVG class → palette color mapping:
 *   .doggo-cls-1  → accent  (fill: none, stroke: outlines + facial features)
 *   .doggo-cls-2  → primary (fill: cream body, tail, ears)
 *   .doggo-cls-3  → white   (fill: #fff — eye whites, intentionally not overridden)
 *
 * Leg fill shapes have no CSS class (default SVG fill: black).
 * They are targeted via path:not([class]) within their parent groups:
 *   - tertiaryColor → leg fill paths (legsColor overrides per-part)
 *
 * Eye pupils may also have unclassed paths, driven by accentColor.
 */
const buildStyles: StyleBuilder<DoggoProps> = (id, props) => {
    const lines: string[] = [];
    const add = createAddRule(lines, id);

    // Global color overrides
    add(".doggo-cls-2", "fill",   props.secondaryColor);  // body, tail, ears
    add(".doggo-cls-1", "fill", props.accentColor);   // all outlines

    // tertiaryColor → leg fill paths (unclassed paths default to black fill)
    add("#legs_back path:not([class])", "fill", props.tertiaryColor);
    add("#legs_front path:not([class])", "fill", props.tertiaryColor);

    // Facial features always use accentColor
    add("#eyes path:not([class])", "fill", props.accentColor);

    // Per-part overrides
    add("#body .doggo-cls-2",                  "fill", props.bodyColor);
    add("#tail",                         "fill", props.tailColor);
    add("#ears .doggo-cls-2",                  "fill", props.earsColor);
    add("#legs_back path:not([class])",  "fill", props.legsColor);
    add("#legs_front path:not([class])", "fill", props.legsColor);

    return lines.join("\n");
};

export default createCharacter<DoggoProps>("Doggo", DoggoSVG, buildStyles);