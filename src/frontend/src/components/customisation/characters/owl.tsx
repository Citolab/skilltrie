/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import OwlSVG from "@/assets/characters/owl.svg?react";
import { createAddRule, createCharacter, type BaseCharacterProps, type StyleBuilder } from "./character-base";

/**
 * Per-part color overrides for the owl character.
 * All props are optional; unset parts fall back to the palette color for their group.
 */
export interface OwlProps extends BaseCharacterProps {
    bodyColor?: string;
    wingsColor?: string;
    headColor?: string;
    beakColor?: string;
    legsColor?: string;
}

/**
 * SVG class → palette color mapping:
 *   .owl-cls-1  → accent    (fill: none, stroke: outlines)
 *   .owl-cls-2  → secondary (fill+stroke: amber/brown — face markings)
 *   .owl-cls-3  → white     (fill+stroke: #fff — eye whites, intentionally not overridden)
 *   .owl-cls-4  → secondary (fill: amber/brown — body detail spots)
 *   .owl-cls-5  → tertiary  (fill: dark grey — beak)
 *   .owl-cls-6  → primary   (fill: light amber — body, head)
 *
 * Wing fill shapes have no CSS class (default SVG fill: black).
 * They are targeted via path:not([class]) within #wings:
 *   - secondaryColor → wing fill paths (wingsColor overrides per-part)
 */
const buildStyles: StyleBuilder<OwlProps> = (id, props) => {
    const lines: string[] = [];
    const add = createAddRule(lines, id);

    // Global color overrides
    add(".owl-cls-6", "fill",   props.secondaryColor);   // body, head
    add(".owl-cls-2", "fill",   props.primaryColor);  // face markings
    add(".owl-cls-2", "stroke", props.primaryColor);
    add(".owl-cls-4", "fill",   props.primaryColor);  // body detail spots (same hue as .owl-cls-2)
    add(".owl-cls-5", "fill",   props.tertiaryColor);   // beak
    add(".owl-cls-1", "stroke", props.accentColor);     // all outlines
    add(".owl-cls-1", "fill", props.accentColor);     // all outlines


    // primaryColor → wing fill paths (unclassed paths default to black fill)
    add("#wings path:not([class])", "fill", props.primaryColor);

    // tertiaryColor → leg fill paths (unclassed paths default to black fill)
    add("#legs path:not([class])", "fill", props.tertiaryColor);

    // Facial features always use accentColor (eye pupils via .owl-cls-1, already covered above)

    // Per-part overrides
    add("#body .owl-cls-6",              "fill", props.bodyColor);
    add("#head .owl-cls-6",              "fill", props.headColor);
    add("#wings path:not([class])",  "fill", props.wingsColor);
    add("#beak .owl-cls-5",              "fill", props.beakColor);
    add("#legs path:not([class])",   "fill", props.legsColor);

    return lines.join("\n");
};

export default createCharacter<OwlProps>("Owl", OwlSVG, buildStyles);