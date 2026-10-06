/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import LionSVG from "@/assets/characters/lion.svg?react";
import { createAddRule, createCharacter, type BaseCharacterProps, type StyleBuilder } from "./character-base";

/**
 * The lion SVG has no semantic group IDs, so only global palette colors are supported.
 * Per-part overrides are not possible without adding IDs to the SVG.
 */
export type LionProps = BaseCharacterProps;

/**
 * SVG class → palette color mapping:
 *   .lion-cls-1  → accent    (fill: none, stroke: outlines)
 *   .lion-cls-2  → white     (fill: #fff — intentionally not overridden)
 *   .lion-cls-3  → primary   (fill: amber/orange body)
 *   .lion-cls-4  → secondary (fill: terracotta details)
 */
const buildStyles: StyleBuilder<LionProps> = (id, props) => {
    const lines: string[] = [];
    const add = createAddRule(lines, id);

    // Global color overrides
    add(".lion-cls-3", "fill",   props.secondaryColor);   // body
    add(".lion-cls-4", "fill",   props.primaryColor);  // detail markings
    add(".lion-cls-1", "stroke", props.accentColor);    // all outlines

    return lines.join("\n");
};

export default createCharacter<LionProps>("Lion", LionSVG, buildStyles);