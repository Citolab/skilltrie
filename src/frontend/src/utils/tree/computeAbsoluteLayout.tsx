/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import type { Node, Edge } from "@xyflow/react";
import { getNodeDomain } from "./getNodeDomain";
import { computeRelativeLayout } from "./computeRelativeLayout";

type NodeId = string;
type DomainId = string;

type Position = { x: number; y: number };
type NodePositioning = Map<NodeId, Position>;

type DomainWidth = Map<DomainId, number>;
type RelativeNodePositioning = Map<NodeId, Position>;

type DomainOffset = Map<DomainId, number>;

export const X_SPACING = 300;
export const Y_SPACING = 100;
export const GRID_SIZE = 50;

export function computeAbsoluteLayout(
    nodes: Node[],
    edges: Edge[],
    expandedDomains: Set<DomainId>,
    domains: DomainId[]
): { positions: NodePositioning; visibleIds: Set<NodeId> } {
    const [domainWidth, relativeNodePositioning] = computeRelativeLayout(nodes, edges, domains);

    const domainOffset = computeDomainOffset(expandedDomains, domainWidth, domains);

    const visibleNodes = replaceHiddenNodes(nodes, expandedDomains);

    const absoluteNodePositioning = relativeToAbsoluteLayout(
        visibleNodes,
        domainOffset,
        relativeNodePositioning
    );

    const visibleNodeIds: Set<string> = new Set(visibleNodes.map((n) => n.id));

    return { positions: absoluteNodePositioning, visibleIds: visibleNodeIds };
}

/*
Removes nodes which are irrelevant;
- Topic nodes belonging to a collapsed domain,
- Domain nodes belonging to an expanded domain

We have to do this here, because computeRelativeLayout requires every node
in order to make a proper layout.
*/
function replaceHiddenNodes(nodes: Node[], expandedDomains: Set<DomainId>): Node[] {
    const withoutExpandedDomainNodes = nodes.filter(
        (node) => node.type !== "domain" || !expandedDomains.has(node.data.domainKey)
    );

    const visibleNodes = withoutExpandedDomainNodes.filter((node) => {
        const domain = getNodeDomain(node);
        if (node.type !== "domain" && !expandedDomains.has(domain)) {
            return false;
        }
        return true;
    });

    return visibleNodes;
}

/*
Uses the relative node positions in combination with the domain
offset to decide their absolute positions.
*/
function relativeToAbsoluteLayout(
    nodes: Node[],
    domainOffset: DomainOffset,
    relativeNodePositioning: RelativeNodePositioning
): NodePositioning {
    const nodePositioning: NodePositioning = new Map();

    // No clue why this is needed in all honesty, but this aligns the domain nodes with the topic nodes.
    const domainOffsetY = 10;

    for (const node of nodes) {
        const domain = getNodeDomain(node);

        const domainPosition = { x: 0, y: domainOffsetY };
        const position = relativeNodePositioning.get(node.id) ?? domainPosition;
        position.x += domainOffset.get(domain)!;

        nodePositioning.set(node.id, position);
    }

    return nodePositioning;
}

/*
Decides the domain offset for each domain, taking into account
that collapsed domains take up less space.
*/
function computeDomainOffset(
    expandedDomains: Set<DomainId>,
    domainWidth: DomainWidth,
    domains: string[]
): DomainOffset {
    const domainOffset: DomainOffset = new Map();

    const collapsedWidth = X_SPACING; // The relative offset if the domain is collapsed.
    let offset = 0;
    let previousDomain = "";

    for (const domain of domains) {
        offset += expandedDomains.has(previousDomain)
            ? domainWidth.get(previousDomain)!
            : collapsedWidth;

        domainOffset.set(domain, offset);
        previousDomain = domain;
    }

    return domainOffset;
}
