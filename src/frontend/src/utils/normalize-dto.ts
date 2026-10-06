/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

/** add default fields to a partial dto object, given a default object for your dto type  */
export default function NormalizeDto<T extends object>(
    dto: Partial<T> | null,
    defaults: T
): T | null {
    if (dto === null || dto === undefined) return null;

    return { ...defaults, ...dto };
}
