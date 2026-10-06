/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

/**
 * How SVG color customization works without editing the SVG files:
 *
 * SVGs exported from Illustrator carry their own <style> block, e.g.:
 *   .cls-1 { fill: #21adcb; }   ← specificity (0,1,0)
 *
 * Each character component wraps the SVG in a <div id="char-xyz"> and injects
 * a <style> tag with scoped rules:
 *   #char-xyz .cls-1 { fill: blue; }      ← specificity (1,1,0) — overrides global class
 *   #char-xyz #body .cls-1 { fill: red; } ← specificity (1,1,1) — overrides per-part
 *
 * This means the SVG files stay untouched; all color logic lives in TSX.
 */

import { useId, type ComponentType } from "react";
import { CHARACTER_PALETTES, type PaletteName } from "@/assets/characters/palettes";

export interface BaseCharacterProps {
    palette?: PaletteName;
    primaryColor?: string;
    secondaryColor?: string;
    tertiaryColor?: string;
    accentColor?: string;
    className?: string;
}

/** Passes through any CSS color value; converts Tailwind tokens like "blue-500" → "var(--color-blue-500)". */
export function resolveColor(color: string | undefined): string | undefined {
    if (!color) return undefined;
    if (color.startsWith("#") || color.startsWith("rgb") || color.startsWith("hsl") || color.startsWith("var("))
        return color;
    return `var(--color-${color})`;
}

/**
 * Returns an `add(selector, property, color)` function that appends a scoped CSS rule
 * (`#${id} ${selector} { ${property}: ${resolved}; }`) to `lines` when `color` resolves.
 * Mutates the passed-in array — caller owns it and reads it back via `lines.join("\n")`.
 */
export function createAddRule(lines: string[], id: string) {
    return (selector: string, property: string, color: string | undefined) => {
        const resolved = resolveColor(color);
        if (resolved) lines.push(`#${id} ${selector} { ${property}: ${resolved}; }`);
    };
}

/**
 * Signature for each character's `buildStyles` function: takes the wrapper id and the
 * palette-resolved props, returns the scoped CSS string injected into the wrapper.
 */
export type StyleBuilder<P extends BaseCharacterProps> =
    (id: string, props: Omit<P, "className" | "palette">) => string;

/** React's useId() returns IDs containing colons (e.g. ":r0:"), which are invalid in CSS selectors. */
export function useCharacterId(): string {
    const rawId = useId();
    return `char-${rawId.replace(/:/g, "")}`;
}

/**
 * Merges palette colors with explicit prop overrides.
 * Props spread last, so individual color props always win over the palette.
 */
export function resolvePalette<T extends Omit<BaseCharacterProps, "palette" | "className">>(
    palette: PaletteName | undefined,
    overrides: T,
): T {
    const paletteColors = palette ? CHARACTER_PALETTES[palette] : {};
    return { ...paletteColors, ...overrides };
}

interface CharacterWrapperProps {
    id: string;
    styles: string;
    className?: string;
    children: React.ReactNode;
}

/** Renders the scoped wrapper div and injects the generated <style> tag before the SVG. */
export function CharacterWrapper({ id, styles, className, children }: CharacterWrapperProps) {
    return (
        <div id={id} className={className}>
            {styles && <style>{styles}</style>}
            {children}
        </div>
    );
}

/**
 * Factory for character components. Wires up id generation, palette resolution,
 * and the wrapper so each character file only needs its SVG and a buildStyles function:
 *
 *   export default createCharacter<CatProps>("Cat", CatSVG, buildStyles);
 *
 * The `name` is used as the React DevTools displayName.
 */
export function createCharacter<P extends BaseCharacterProps>(
    name: string,
    SVG: ComponentType,
    buildStyles: StyleBuilder<P>,
) {
    function Character(props: P) {
        const { palette, className, ...colorProps } = props;
        const id = useCharacterId();
        const resolved = resolvePalette(palette, colorProps);
        const styles = buildStyles(id, resolved);
        return (
            <CharacterWrapper id={id} styles={styles} className={className}>
                <SVG />
            </CharacterWrapper>
        );
    }
    Character.displayName = name;
    return Character;
}
