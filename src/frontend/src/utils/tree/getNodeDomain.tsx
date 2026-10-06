/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import type { Node } from "@xyflow/react";

/*
This file handles getting node domain. It is currently not a property,
but it should be later. You can then simply replace all occurances with
the property access.
*/

export function getNodeDomain(node: Node): string {
    return node.type === "domain"
        ? node.data.domainKey
        : (node.data as any)?.objectInfo?.ancestorName;
}
