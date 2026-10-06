/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import type { Node, Edge } from "@xyflow/react";
import { computeAbsoluteLayout } from "./computeAbsoluteLayout";

export const topicNodeWidth = 250;
export const topicNodeHeight = 75;
export const domainNodeWidth = 250;
export const domainNodeHeight = 250;
export const DOMAIN_SPACING = domainNodeWidth + 100; // base center-to-center spacing between domain nodes (collapsed)
export const DOMAIN_Y = 0; // y position of domain nodes
export const TOPIC_GAP = 60; // space between domain right edge and first topic column
export const TOPIC_COL_GAP = 50; // horizontal gap between topic columns
export const TOPIC_ROW_GAP = 50; // vertical gap between topics in the same column

/** Attaches edge/handle direction metadata. Actual positions are computed by computeLayout. */
export const layoutMetro = (nodes: any[], edges: any[]) => ({
    nodes: nodes.map((node: any) => ({
        ...node,
        targetPosition: "left",
        sourcePosition: "right",
        position: { x: 0, y: DOMAIN_Y },
    })),
    edges,
});

type NodeId = string;
type DomainId = string;

type Position = { x: number; y: number };
type NodePositioning = Map<NodeId, Position>;

/**
 * Pure layout computation: given current nodes, edges, and which domains are expanded,
 * returns the absolute position of every visible node and the set of visible node IDs.
 */
export function computeLayout(
    nodes: Node[],
    edges: Edge[],
    expandedDomains: Set<DomainId>,
    domains: string[]
): { positions: NodePositioning; visibleIds: Set<NodeId> } {
    return computeAbsoluteLayout(nodes, edges, expandedDomains, domains);
}

/**
 * Pure edge computation: filters edges to only those between visible nodes and
 * builds bridge edges that bypass hidden domain nodes when a domain is expanded.
 */
export function computeVisibleEdges(
    edges: Edge[],
    visibleIds: Set<NodeId>,
    expandedDomains: Set<DomainId>
): Edge[] {
    const filtered = edges.filter((e) => {
        if (!visibleIds.has(e.source) || !visibleIds.has(e.target)) return false;
        
        const sourceDomainKey = (e.data as any)?.sourceDomainKey;
        if (sourceDomainKey && expandedDomains.has(sourceDomainKey)) return false;
        return true;
    });

    const bridgeEdges: Edge[] = [];

    // When a topic's target domain is expanded (domain node hidden), bridge directly
    // from the source topic to each of the domain's entry-point topics.
    for (const e of edges) {
        if (e.type !== "crossDomain") continue;
        const targetDomainKey = e.target.replace(/^domain-/, "");
        if (!expandedDomains.has(targetDomainKey) || !visibleIds.has(e.source)) continue;
        for (const de of edges) {
            if (
                de.source === `domain-${targetDomainKey}` &&
                de.id.startsWith("domain-first-") &&
                visibleIds.has(de.target)
            ) {
                bridgeEdges.push({
                    id: `bridge-${e.source}-to-${de.target}`,
                    source: e.source,
                    target: de.target,
                    type: "crossDomain",
                });
            }
        }
    }

    // When domain-B is expanded, the domain-A → domain-B chain edge loses its target.
    // Bridge domain-A directly to domain-B's entry-point topics instead.
    for (const e of edges) {
        if (!e.id.startsWith("domain-seq-")) continue;
        const targetDomainKey = e.target.replace(/^domain-/, "");
        if (!expandedDomains.has(targetDomainKey) || !visibleIds.has(e.source)) continue;
        for (const de of edges) {
            if (
                de.source === `domain-${targetDomainKey}` &&
                de.id.startsWith("domain-first-") &&
                visibleIds.has(de.target)
            ) {
                bridgeEdges.push({
                    id: `bridge-${e.source}-to-${de.target}`,
                    source: e.source,
                    target: de.target,
                    type: "crossDomain",
                });
            }
        }
    }

    return [...filtered, ...bridgeEdges];
}

/**
 * Builds the full edge set from raw topic dependency edges and the node list.
 * Produces intra-domain edges, cross-domain bridges, the sequential domain chain,
 * and domain → first-topic edges. Pure function — no React dependency.
 */
export function buildEdges(
    topicEdges: Array<{ id: string; source: string; target: string; weight?: number }>,
    nodes: any[],
    domains: string[]
): any[] {
    const nodeIdToDomain = new Map<string, string>();
    nodes.forEach((node) => {
        if (node.type === "domain") return;
        const domain = node.data?.objectInfo?.ancestorName;
        if (domain) nodeIdToDomain.set(node.id, domain);
    });

    const seenCross = new Set<string>();
    const crossDomainEdges = topicEdges.flatMap((edge) => {
        const srcDomain = nodeIdToDomain.get(edge.source);
        const tgtDomain = nodeIdToDomain.get(edge.target);
        if (!srcDomain || !tgtDomain || srcDomain === tgtDomain) return [];
        const key = `${edge.source}->${tgtDomain}`;
        if (seenCross.has(key)) return [];
        seenCross.add(key);
        return [
            {
                id: `cross-${edge.source}-to-${tgtDomain}`,
                source: edge.source,
                target: `domain-${tgtDomain}`,
                type: "crossDomain",
            },
        ];
    });

    const domainChainEdges = domains.slice(0, -1).map((domain, i) => ({
        id: `domain-seq-${i}`,
        source: `domain-${domain}`,
        target: `domain-${domains[i + 1]}`,
        data: { sourceDomainKey: domain },
    }));

    // Connect each domain node to the topics with no intra-domain prerequisites (in-degree 0).
    const domainToFirstTopicEdges: any[] = [];
    domains.forEach((domain) => {
        const domainTopics = nodes.filter(
            (n) => n.type !== "domain" && nodeIdToDomain.get(n.id) === domain
        );
        const domainTopicIds = new Set(domainTopics.map((n: any) => n.id));
        const hasIncoming = new Set<string>();
        topicEdges.forEach((e) => {
            if (domainTopicIds.has(e.source) && domainTopicIds.has(e.target))
                hasIncoming.add(e.target);
        });
        domainTopics.forEach((n: any) => {
            if (!hasIncoming.has(n.id)) {
                domainToFirstTopicEdges.push({
                    id: `domain-first-${n.id}`,
                    source: `domain-${domain}`,
                    target: n.id,
                });
            }
        });
    });

    return [...topicEdges, ...crossDomainEdges, ...domainChainEdges, ...domainToFirstTopicEdges];
}
