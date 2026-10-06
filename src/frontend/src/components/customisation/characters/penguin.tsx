/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import PenguinSVG from "@/assets/characters/penguin.svg?react";
import { createAddRule, createCharacter, type BaseCharacterProps, type StyleBuilder } from "./character-base";

/**
 * Per-part color overrides for the penguin character.
 * All props are optional; unset parts fall back to the palette color for their group.
 */
export interface PenguinProps extends BaseCharacterProps {
    bodyColor?: string;
    stomachColor?: string;
    wingsColor?: string;
}

/**
 * SVG class → palette color mapping:
 *   .penguin-cls-1  → accent    (fill: none, stroke: outlines + eye pupils)
 *   .penguin-cls-2  → tertiary  (fill+stroke: amber/orange — beak, wing accents)
 *   .penguin-cls-3  → secondary (fill: none, stroke: grey — stomach outline)
 *   .penguin-cls-4  → white     (fill: #fff — eye whites, intentionally not overridden)
 *   .penguin-cls-5  → secondary (fill: grey — stomach fill)
 *   .penguin-cls-6  → primary   (fill: near-black — main body)
 */
const buildStyles: StyleBuilder<PenguinProps> = (id, props) => {
    const lines: string[] = [];
    const add = createAddRule(lines, id);

    // Global color overrides
    add(".penguin-cls-6", "fill",   props.primaryColor);   // main body
    add(".penguin-cls-5", "fill",   props.secondaryColor);  // stomach fill
    add(".penguin-cls-3", "stroke", props.secondaryColor);  // stomach outline
    add(".penguin-cls-2", "fill",   props.tertiaryColor);   // beak + wing accents
    add(".penguin-cls-2", "stroke", props.tertiaryColor);
    add(".penguin-cls-1", "stroke", props.accentColor);    // all outlines + eye pupils

    // Facial features always use accentColor (eye pupils via .penguin-cls-1, already covered above)

    // Per-part overrides
    add("#body .penguin-cls-6",     "fill",   props.bodyColor);
    add("#stomach .penguin-cls-5",  "fill",   props.stomachColor);
    add("#stomach .penguin-cls-3",  "stroke", props.stomachColor);
    add("#wings .penguin-cls-2",    "fill",   props.wingsColor);
    add("#wings .penguin-cls-2",    "stroke", props.wingsColor);

    return lines.join("\n");
};

export default createCharacter<PenguinProps>("Penguin", PenguinSVG, buildStyles);