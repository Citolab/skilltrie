/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

export interface StampOption {
    name: string;
    path: string;
}

const normalizedBaseURL: string = (import.meta.env.VITE_BASE_PATH as string).replace("/\/$/", "");

export const stamps: StampOption[] = Object.keys(import.meta.glob("/public/badges/*")).map((s) => {
    const name: string = s.split("/").pop()!.replace(".svg", "");
    return {
        name: name,
        path: parseStampImage(name),
    };
});

/**
 * Resolves a badge's stamp name to its image URL.
 *
 * The path of the images are standardly stored without the .svg,
 * but we still do want to render paths which have for some reason .svg as suffix by mistake.
 *
 * @param stamp - The stamp name from the badge, e.g. "welcome stamp".
 * @returns A URL usable as an `<img src>`.
 */
export function parseStampImage(stamp?: string) {
    return stamp
        ? normalizedBaseURL.concat(`/badges/${stamp.replace(".svg", "")}.svg`)
        : "https://cdn-icons-png.flaticon.com/512/1534/1534225.png";
}
