/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import PigeonSVG from "@/assets/characters/pigeon.svg?react";
import { createAddRule, createCharacter, type BaseCharacterProps, type StyleBuilder } from "./character-base";

/**
 * Per-part color overrides for the pigeon character.
 * All props are optional; unset parts fall back to the palette color for their group.
 */
export interface PigeonProps extends BaseCharacterProps {
    bodyColor?: string;
    legsColor?: string;
    beakColor?: string;
}

/**
 * SVG class → palette color mapping:
 *   .pigeon-cls-1  → primary   (fill+stroke: lavender — body, head, legs)
 *   .pigeon-cls-2  → accent    (fill: none, stroke: outlines + eye pupils)
 *   .pigeon-cls-3  → white     (fill+stroke: #fff — eye whites, intentionally not overridden)
 *   .pigeon-cls-4  → tertiary  (fill: none, stroke: grey — beak stroke)
 *   .pigeon-cls-5  → secondary (fill: light blue — wing/head highlights)
 *   .pigeon-cls-6  → tertiary  (fill: grey — beak fill)
 */
const buildStyles: StyleBuilder<PigeonProps> = (id, props) => {
    const lines: string[] = [];
    const add = createAddRule(lines, id);

    // Global color overrides
    add(".pigeon-cls-1", "fill",   props.primaryColor);   // body, head, legs
    add(".pigeon-cls-1", "stroke", props.primaryColor);
    add(".pigeon-cls-5", "fill",   props.secondaryColor);  // wing and head highlights
    add(".pigeon-cls-6", "fill",   props.tertiaryColor);   // beak fill
    add(".pigeon-cls-4", "stroke", props.tertiaryColor);   // beak stroke
    add(".pigeon-cls-2", "stroke", props.accentColor);    // all outlines + eye pupils

    // Facial features always use accentColor (eye pupils via .pigeon-cls-2, already covered above)

    // Per-part overrides
    add("#body .pigeon-cls-1",  "fill",   props.bodyColor);
    add("#body .pigeon-cls-1",  "stroke", props.bodyColor);
    add("#legs .pigeon-cls-1",  "fill",   props.legsColor);
    add("#legs .pigeon-cls-1",  "stroke", props.legsColor);
    add("#beak .pigeon-cls-6",  "fill",   props.beakColor);
    add("#beak .pigeon-cls-4",  "stroke", props.beakColor);

    return lines.join("\n");
};

export default createCharacter<PigeonProps>("Pigeon", PigeonSVG, buildStyles);