/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import CatSVG from "@/assets/characters/cat.svg?react";
import { createAddRule, createCharacter, type BaseCharacterProps, type StyleBuilder } from "./character-base";

/**
 * Per-part color overrides for the cat character.
 * All props are optional; unset parts fall back to the palette color for their group.
 */
export interface CatProps extends BaseCharacterProps {
    bodyColor?: string;
    headColor?: string;
    tailColor?: string;
    legsColor?: string;
}

/**
 * SVG class → palette color mapping:
 *   .cat-cls-1  → accent  (fill: none, stroke: outlines + facial features)
 *   .cat-cls-2  → white   (fill: #fff — eye whites, intentionally not overridden)
 *   .cat-cls-3  → primary (fill: grey body, head, tail)
 *
 * Leg fill shapes and facial pupils have no CSS class (default SVG fill: black).
 * They are targeted via path:not([class]) within their parent groups:
 *   - tertiaryColor → leg fill paths (legsColor overrides per-part)
 *   - accentColor   → nose and face pupils
 */
const buildStyles: StyleBuilder<CatProps> = (id, props) => {
    const lines: string[] = [];
    const add = createAddRule(lines, id);

    // Global color overrides
    add(".cat-cls-3", "fill",   props.secondaryColor);  // body, head, tail
    add(".cat-cls-1", "fill", props.accentColor);   // all outlines

    // tertiaryColor → leg fill paths (unclassed paths default to black fill)
    add("#legs_back path:not([class])", "fill", props.tertiaryColor);
    add("#legs_front path:not([class])", "fill", props.tertiaryColor);

    // accentColor → nose fill and face pupils (also unclassed paths)
    add("#nose path:not([class])",      "fill", props.accentColor);
    add("#face path:not([class])",      "fill", props.accentColor);

    // Per-part overrides
    add("#body .cat-cls-3",                        "fill", props.bodyColor);
    add("#head .cat-cls-3",                        "fill", props.headColor);
    add("#tail",                               "fill", props.tailColor);
    add("#legs_back path:not([class])",        "fill", props.legsColor);
    add("#legs_front path:not([class])",       "fill", props.legsColor);

    return lines.join("\n");
};

export default createCharacter<CatProps>("Cat", CatSVG, buildStyles);