/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import DogSVG from "@/assets/characters/dog.svg?react";
import { createAddRule, createCharacter, type BaseCharacterProps, type StyleBuilder } from "./character-base";

/**
 * Per-part color overrides for the dog character.
 * All props are optional; unset parts fall back to the palette color for their group
 * (primary / secondary / tertiary). Facial features always use accentColor.
 */
export interface DogProps extends BaseCharacterProps {
    tailColor?: string;
    bodyColor?: string;
    earsColor?: string;
    stomachColor?: string;
    snoutColor?: string;
    legsColor?: string;
}

/**
 * Builds a scoped CSS string for the given wrapper id.
 *
 * Two-tier override pattern:
 *   1. Global rules target SVG class names (.dog-cls-x) and set the palette color for
 *      every element using that class — specificity (1,1,0).
 *   2. Per-part rules add an ID selector (#part .dog-cls-x) which raises specificity to
 *      (1,1,1), overriding the global rule for that part only.
 *
 * SVG class → palette color mapping (from Illustrator export):
 *   .dog-cls-1 / .dog-cls-3  → primary   (body, tail)
 *   .dog-cls-6 / .dog-cls-2  → secondary (ears, snout, stomach)
 *   .dog-cls-5 / .dog-cls-4  → tertiary  (legs) / accentColor (facial features)
 */
const buildStyles: StyleBuilder<DogProps> = (id, props) => {
    const lines: string[] = [];
    const add = createAddRule(lines, id);

    // Global color overrides — apply the palette color to all elements of each class
    add(".dog-cls-1", "fill",   props.primaryColor);   // body + tail
    add(".dog-cls-3", "stroke", props.primaryColor);
    add(".dog-cls-6", "fill",   props.secondaryColor); // ears + snout + stomach
    add(".dog-cls-2", "stroke", props.secondaryColor);
    add(".dog-cls-5", "fill",   props.accentColor);  // legs (and nose/sunglasses before accent overrides below)
    add(".dog-cls-4", "stroke", props.tertiaryColor);

    // Per-part overrides — higher specificity wins over the global rules above
    add("#tail .dog-cls-1",       "fill",   props.tailColor);
    add("#tail .dog-cls-3",       "stroke", props.tailColor);

    add("#body .dog-cls-1",       "fill",   props.bodyColor);
    add("#body .dog-cls-3",       "stroke", props.bodyColor);

    add("#ear1 .dog-cls-6",       "fill",   props.earsColor);
    add("#ear1 .dog-cls-2",       "stroke", props.earsColor);
    add("#ear2 .dog-cls-6",       "fill",   props.earsColor);
    add("#ear2 .dog-cls-2",       "stroke", props.earsColor);

    add("#stomach .dog-cls-6",    "fill",   props.stomachColor);
    add("#stomach .dog-cls-2",    "stroke", props.stomachColor);

    add("#snout .dog-cls-6",      "fill",   props.snoutColor);
    add("#snout .dog-cls-2",      "stroke", props.snoutColor);

    add("#legs .dog-cls-5",       "fill",   props.legsColor);
    add("#legs .dog-cls-4",       "stroke", props.legsColor);

    // Facial features are always driven by accentColor, overriding the tertiary global rule
    add("#nose .dog-cls-5",       "fill",   props.accentColor);
    add("#nose .dog-cls-4",       "stroke", props.accentColor);
    add("#sunglasses .dog-cls-5", "fill",   props.accentColor);
    add("#sunglasses .dog-cls-4", "stroke", props.accentColor);
    add("#mouth",             "stroke", props.accentColor);

    return lines.join("\n");
};

export default createCharacter<DogProps>("Dog", DogSVG, buildStyles);
