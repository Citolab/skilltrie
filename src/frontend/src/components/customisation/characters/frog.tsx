/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import FrogSVG from "@/assets/characters/frog.svg?react";
import { createAddRule, createCharacter, type BaseCharacterProps, type StyleBuilder } from "./character-base";

/**
 * Per-part color overrides for the frog character.
 * All props are optional; unset parts fall back to the palette color for their group
 * (primary / secondary / tertiary). Facial features always use accentColor.
 */
export interface FrogProps extends BaseCharacterProps {
    bodyColor?: string;
    stomachColor?: string;
    legsColor?: string;
}

/**
 * Builds a scoped CSS string for the given wrapper id.
 *
 * SVG class → palette color mapping (from Illustrator export):
 *   .frog-cls-4           → primary   (body)
 *   .frog-cls-2           → secondary (stomach)
 *   .frog-cls-1 / .frog-cls-5  → tertiary  (legs) / accentColor (eyes, mouth)
 *   .frog-cls-3           → white (eye whites) — intentionally never overridden
 */
const buildStyles: StyleBuilder<FrogProps> = (id, props) => {
    const lines: string[] = [];
    const add = createAddRule(lines, id);

    // Global color overrides — apply the palette color to all elements of each class
    add(".frog-cls-4", "fill",   props.primaryColor);  // body
    add(".frog-cls-2", "fill",   props.secondaryColor); // stomach
    add(".frog-cls-1", "fill",   props.accentColor);  // legs (and eyes before accent overrides below)
    add(".frog-cls-5", "stroke", props.tertiaryColor);  // legs + mouth outlines

    // Per-part overrides — higher specificity wins over the global rules above
    add("#body .frog-cls-4",    "fill",   props.bodyColor);

    add("#stomach .frog-cls-2", "fill",   props.stomachColor);

    add("#legs .frog-cls-1",    "fill",   props.legsColor);
    add("#legs .frog-cls-5",    "stroke", props.legsColor);

    // Facial features are always driven by accentColor, overriding the tertiary global rule
    add("#mouth",          "stroke", props.accentColor);
    add("#eyes .frog-cls-1",    "fill",   props.accentColor);
    add("#eyes .frog-cls-5",    "stroke", props.accentColor);

    return lines.join("\n");
};

export default createCharacter<FrogProps>("Frog", FrogSVG, buildStyles);
