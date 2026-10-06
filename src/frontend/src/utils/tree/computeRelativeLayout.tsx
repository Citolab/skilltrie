/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import type { Node, Edge } from "@xyflow/react";
import { getNodeDomain } from "./getNodeDomain";
import { GRID_SIZE, X_SPACING, Y_SPACING } from "./computeAbsoluteLayout";

type NodeId = string;
type DomainId = string;

type Depth = number;
type Width = number;

type AdjacencyMap = Map<NodeId, NodeId[]>;
type DepthMap = Map<NodeId, number>;
type DepthGrid = Map<Depth, Map<Width, NodeId>>;

type Position = { x: number; y: number };
type NodePositioning = Map<NodeId, Position>;

type DomainWidth = Map<DomainId, number>;
type RelativeNodePositioning = Map<NodeId, Position>;

type DomainOffset = Map<DomainId, number>;

/* 
Calculates the node layout assuming that everything is expanded.
Expanded domain logic is handled in computation of the Absolute layout.
*/
export function computeRelativeLayout(
    nodes: Node[],
    edges: Edge[],
    domains: string[]
): [DomainWidth, RelativeNodePositioning] {
    if (nodes.length == 0 || edges.length == 0) {
        return [new Map<DomainId, number>(), new Map<string, { x: number; y: number }>()];
    }

    const topicNodes = nodes.filter((n) => n.type !== "domain");
    const topicEdges = edges.filter((e) => {
        const source = nodes.find((i) => i.id === e.source)!;
        const target = nodes.find((i) => i.id === e.target)!;
    
        return source.type !== "domain" && target.type !== "domain";
    });

    // PREPROCESSING

    const [predecessors, successors] = calculateLinkedNodes(topicEdges, topicNodes);

    const [mainChain, earChains, branches] = findLongestPathLengths(topicNodes, successors);

    const depthMap = calculateNodeDepths(predecessors, successors, topicNodes);

    const depthGrid = assignLayersByPath(depthMap, mainChain, earChains, branches);

    // From this part onwards, we transfer from node Orderings to node Positionings
    const nodePositioning = assignInitialCordinates(depthGrid);

    const gridSize = GRID_SIZE;
    snapToGrid(nodePositioning, gridSize);

    // After this, the node positions are 'locked' in place

    const domainAbsoluteOffset = calculateDomainOffsets(nodePositioning, topicNodes, mainChain);

    const relativeNodePositioning = calculateRelativeNodePositioning(
        nodePositioning,
        topicNodes,
        domainAbsoluteOffset
    );
    const domainWidth = calculateDomainWidths(nodePositioning, topicNodes, mainChain, domains);

    return [domainWidth, relativeNodePositioning];
}

/*
Uses the absolute node positions and domain offsets to shift them
to their position relative to the domain start 'pointer' (the offset).
*/
function calculateRelativeNodePositioning(
    nodePositioning: NodePositioning,
    nodes: Node[],
    domainOffset: DomainOffset
): RelativeNodePositioning {
    const relativeNodePositioning: RelativeNodePositioning = new Map();

    for (const node of nodes) {
        const absolutePosition = nodePositioning.get(node.id)!;
        if (!absolutePosition) {
            continue;
        }

        const nodeDomain = getNodeDomain(node);
        const offset = domainOffset.get(nodeDomain)!;

        const relativePosition: Position = {
            x: absolutePosition.x - offset,
            y: absolutePosition.y,
        };

        relativeNodePositioning.set(node.id, relativePosition);
    }

    return relativeNodePositioning;
}

/*
Determines where each domain starts, its 'offset'. It traverses the main chain.
This means it makes the following assumption:
- For each depth, every node in the layer belongs to the same domain
*/
function calculateDomainOffsets(
    nodePositioning: NodePositioning,
    nodes: Node[],
    mainChain: NodeId[]
): DomainOffset {
    const domainOffset: DomainOffset = new Map();

    let highestPosition = 0;
    let currentDomain: string = "";

    for (const nodeId of mainChain) {
        const node = nodes.find((i) => i.id === nodeId)!;

        const nodeDomain = getNodeDomain(node);

        const position = nodePositioning.get(nodeId)!;
        highestPosition = position.x;

        if (nodeDomain !== currentDomain) {
            currentDomain = nodeDomain;
            const offset = highestPosition;
            domainOffset.set(currentDomain, offset);
        }
    }

    return domainOffset;
}

function calculateDomainWidths(
    nodePositioning: NodePositioning,
    nodes: Node[],
    mainChain: NodeId[],
    domains: string[]
): DomainWidth {
    const domainMin = new Map<DomainId, number>();
    const domainMax = new Map<DomainId, number>();

    for (const nodeId of mainChain) {
        const node = nodes.find((n) => n.id === nodeId)!;

        const domain = getNodeDomain(node);
        const pos = nodePositioning.get(nodeId)!;

        domainMin.set(domain, Math.min(domainMin.get(domain) ?? pos.x, pos.x));

        domainMax.set(domain, Math.max(domainMax.get(domain) ?? pos.x, pos.x));
    }

    const domainWidth: DomainWidth = new Map();
    const padding = X_SPACING;

    for (const domain of domains) {
        const min = domainMin.get(domain);
        const max = domainMax.get(domain);

        if (min === undefined || max === undefined) {
            domainWidth.set(domain, padding);
            continue;
        }

        domainWidth.set(domain, max - min + padding);
    }

    return domainWidth;
}

// Uses all the discovered paths to assign the relative node positions (in the form of a grid)
function assignLayersByPath(
    depthMap: DepthMap,
    mainChain: NodeId[],
    earChains: NodeId[][],
    branches: NodeId[][]
): DepthGrid {
    const depthGrid: DepthGrid = new Map();
    const assigned = new Set<NodeId>();

    for (const node of mainChain) {
        const nodeDepth = depthMap.get(node)! + 1;
        depthGrid.set(nodeDepth, new Map());
        depthGrid.get(nodeDepth)!.set(1, node);
        assigned.add(node);
    }

    assignChains(earChains, false);
    assignChains(branches, true);

    function assignChains(chains: NodeId[][], areBranches: boolean) {
        // Branches, unlike ear chains, do not end with a 'connector' node,
        // therefore we have to consider that final node.
        const offByOne = areBranches ? 1 : 0;

        for (const chain of chains) {
            // True first node is index 1, since index 0 is the node on the main chain.
            const firstNode: NodeId = chain[1];
            const firstNodeDepth = depthMap.get(firstNode)! + 1;

            // Find the smallest depth which we can use without intersections
            let smallestWidthForChain = 1;
            for (
                let depthIndex = firstNodeDepth;
                depthIndex < firstNodeDepth + chain.length - offByOne;
                depthIndex++
            ) {
                const layer = depthGrid.get(depthIndex)!;
                const maxWidth = Math.max(...layer.keys());
                smallestWidthForChain = Math.max(maxWidth, smallestWidthForChain);
            }

            for (let nodeIndex = 1; nodeIndex < chain.length - 1 + offByOne; nodeIndex++) {
                const node: NodeId = chain[nodeIndex];
                if (assigned.has(node)) {
                    continue;
                }
                depthGrid.get(firstNodeDepth + nodeIndex - 1)!.set(smallestWidthForChain + 1, node);
                assigned.add(node);
            }
        }
    }

    return depthGrid;
}

function snapToGrid(positioning: NodePositioning, gridSize: number): void {
    for (const [_, pos] of positioning) {
        pos.x = Math.round(Math.round(pos.x) / gridSize) * gridSize;
        pos.y = Math.round(Math.round(pos.y) / gridSize) * gridSize;
    }
}

function assignInitialCordinates(depthGrid: DepthGrid): NodePositioning {
    const positions: NodePositioning = new Map();

    const xSpacing = X_SPACING;
    const ySpacing = Y_SPACING;

    for (const [depth, column] of depthGrid) {
        for (const [lane, node] of column) {
            positions.set(node, {
                x: depth * xSpacing,
                y: lane * ySpacing,
            });
        }
    }

    return positions;
}

function findLongestPathLengths(
    nodes: Node[],
    successors: AdjacencyMap
): [
    NodeId[], // main chain
    NodeId[][], // ear chains
    NodeId[][], // branch chains
] {
    const mainSuccessors = new Map<NodeId, NodeId>();
    const memo = new Map<NodeId, number>();

    function dfs(id: NodeId): number {
        if (memo.has(id)) return memo.get(id)!;

        const children = successors.get(id) ?? [];

        if (children.length === 0) {
            memo.set(id, 1);
            return 1;
        }

        let bestChild = children[0];
        let bestLen = dfs(bestChild);

        for (let i = 1; i < children.length; i++) {
            const c = children[i];
            const len = dfs(c);

            if (len > bestLen) {
                bestLen = len;
                bestChild = c;
            }
        }

        mainSuccessors.set(id, bestChild);
        memo.set(id, bestLen + 1);

        return bestLen + 1;
    }

    for (const n of nodes) {
        dfs(n.id);
    }

    // -------------------------
    // BUILD MAIN CHAIN
    // -------------------------

    let start = nodes
        .map((n) => n.id)
        .reduce((a, b) => ((memo.get(a) ?? 0) > (memo.get(b) ?? 0) ? a : b));

    const mainChain: NodeId[] = [];
    const mainSet = new Set<NodeId>();

    while (start && !mainSet.has(start)) {
        mainChain.push(start);
        mainSet.add(start);
        start = mainSuccessors.get(start)!;
    }

    // -------------------------
    // CLASSIFY CHAINS
    // -------------------------

    const earChains: NodeId[][] = [];
    const branchChains: NodeId[][] = [];

    function explore(node: NodeId, path: NodeId[], leftMain: boolean) {
        const children = successors.get(node) ?? [];

        for (const c of children) {
            const isMain = mainSet.has(c);

            // EAR: only valid if we already left main AND re-enter it
            if (isMain && leftMain) {
                earChains.push([...path, c]);
                continue;
            }

            // backbone transition, ignore
            if (isMain && !leftMain) {
                continue;
            }

            const newLeftMain = leftMain || !isMain;
            const newPath = [...path, c];

            if ((successors.get(c) ?? []).length === 0) {
                branchChains.push(newPath);
            } else {
                explore(c, newPath, newLeftMain);
            }
        }
    }

    for (const m of mainChain) {
        explore(m, [m], false);
    }

    return [mainChain, earChains, branchChains];
}

// BFS longest-path to assign each node its column (= topological depth)
function calculateNodeDepths(
    predecessors: AdjacencyMap,
    successors: AdjacencyMap,
    nodes: Node[]
): DepthMap {
    const inDegree = new Map<NodeId, number>();

    for (const node of nodes) {
        inDegree.set(node.id, predecessors.get(node.id)!.length);
    }

    const depths = new Map<NodeId, number>();
    const queue: string[] = [];
    nodes.forEach((n: Node) => {
        if ((inDegree.get(n.id) ?? 0) === 0) {
            queue.push(n.id);
            depths.set(n.id, 0);
        }
    });

    while (queue.length > 0) {
        const id = queue.shift()!;
        const d = depths.get(id) ?? 0;

        for (const nid of successors.get(id) ?? []) {
            const nd = d + 1;
            if (!depths.has(nid) || depths.get(nid)! < nd) {
                depths.set(nid, nd);
            }
            const deg = inDegree.get(nid)! - 1;
            inDegree.set(nid, deg);
            if (deg === 0) {
                queue.push(nid);
            }
        }
    }
    nodes.forEach((n: Node) => {
        if (!depths.has(n.id)) depths.set(n.id, 0);
    });

    return depths;
}

// Returns in-degrees and out-degrees
function calculateLinkedNodes(edges: Edge[], nodes: Node[]): [AdjacencyMap, AdjacencyMap] {
    const predecessors: AdjacencyMap = new Map();
    const successors: AdjacencyMap = new Map();

    for (const node of nodes) {
        predecessors.set(node.id, []);
        successors.set(node.id, []);
    }

    for (const edge of edges) {
        predecessors.get(edge.target)!.push(edge.source);
        successors.get(edge.source)!.push(edge.target);
    }
    return [predecessors, successors];
}
