/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */
import { useEffect, useState, useCallback, useRef, useMemo } from "react";
import {
    ReactFlow,
    MiniMap,
    useNodesState,
    useEdgesState,
    useReactFlow,
    ReactFlowProvider,
    type Edge,
} from "@xyflow/react";
import "@xyflow/react/dist/style.css";
import MasteredNode from "./nodes/mastered";
import UnlockedNode from "./nodes/unlocked";
import LockedNode from "./nodes/locked";
import DomainNode from "./nodes/domain-node.tsx";
import { GetTopicDependencies, GetUserTopicInfo, GetNextSuggestedTopic } from "../../api/topic.ts";
import type { ScopeEdge, UserScopeInfo, TopicLabel } from "../../types/scope.ts";
import {
    layoutMetro,
    computeLayout,
    computeVisibleEdges,
    buildEdges,
    topicNodeHeight,
    topicNodeWidth,
    domainNodeHeight,
    domainNodeWidth,
} from "../../utils/tree/layoutMetro.tsx";
import { layoutMetroMobile } from "../../utils/tree/layoutMetroMobile.tsx";
import { Tooltip } from "./node-tooltip/node-tooltip.tsx";
import { ArrowsPointingInIcon } from "@heroicons/react/24/outline";
import MetroEdge from "./edges/MetroEdge.tsx";
import CrossDomainEdge from "./edges/CrossDomainEdge.tsx";
import { useBreakPoints } from "../../hooks/useBreakPoints.tsx";
import { useFocusNode } from "./nodes/base-node.tsx";
import { useABTest } from "../../hooks/ab-testing.ts";
import { singleDomainExpandAbTest } from "../../utils/ab-tests/single-domain-expand-ab-test.tsx";
import { Legend } from "../../components/tree/legend.tsx";
import { useHomeTourContext } from "../../contexts/home-tour-context.tsx";

const nodeTypes = {
    mastered: MasteredNode,
    unlocked: UnlockedNode,
    locked: LockedNode,
    domain: DomainNode,
};

const edgeTypes = {
    edge: MetroEdge,
    // Used for edges that bridge domain boundaries (last topic -> next domain, domain -> first topic).
    // Draws a bezier arc above the node cluster so the
    // path never visually passes through unrelated nodes.
    crossDomain: CrossDomainEdge,
};

export type SelectedTopic = {
    topicInfo: UserScopeInfo;
    element: Element;
};

const { nodes: layoutedNodes, edges: layoutedEdges } = layoutMetro([], []);

// Assigns CSS color variables to domains based on their position in domains, so color follows of graph..
const getDomainColorMap: (domains: string[]) => Record<string, string> = (domains: string[]) =>
    Object.fromEntries(domains.map((domain, i) => [domain, `--color-domain-${(i % 4) + 1}`]));

// The Tree component is wrapped in a ReactFlowProvider to provide the necessary context for React Flow components and hooks used within Tree.
// This allows Tree to use hooks like useReactFlow and ensures that all React Flow components within Tree have access to the shared state and functionality provided by the provider.
export default function OuterTree() {
    return (
        <ReactFlowProvider>
            <Tree />
        </ReactFlowProvider>
    );
}

function Tree() {
    const [nodes, setNodes] = useNodesState(layoutedNodes);
    const [edges, setEdges] = useEdgesState(layoutedEdges);
    const [domains, setDomains] = useState<string[]>([]);
    const [shouldSuggestTopic, setShouldSuggestTopic] = useState<boolean>(false);
    const [nodeColorMap, setNodeColorMap] = useState<Map<string, string>>(new Map());
    const [nodeStatusMap, setNodeStatusMap] = useState<Map<string, string>>(new Map());
    const [expandedDomains, setExpandedDomains] = useState<Set<string>>(new Set());
    const [refresh] = useState(0);
    const { tourActive, setTourActive } = useHomeTourContext();

    const domainColorMap = getDomainColorMap(domains);

    const singleDomainExpand = useABTest(singleDomainExpandAbTest);

    const [currentTopicSelected, setCurrentTopicSelected] = useState<SelectedTopic | null>(null);
    const { fitView } = useReactFlow();

    const containerRef = useRef<HTMLDivElement>(null);

    // Tracks the latest computed positions of all visible nodes so focusNode can
    // use the actual rendered position even after the layout has shifted.
    const computedPositionsRef = useRef(new Map<string, { x: number; y: number }>());

    // Toggles a domain open/closed. Other open domains are unaffected.
    // Passed as setExpandedDomain to domain nodes so they can trigger fitView too.
    const handleDomainExpand = useCallback(
        (domain: string, skipFitView = false) => {
            setExpandedDomains((prev) => {
                const next = new Set(prev);
                if (next.has(domain)) next.delete(domain);
                else if (singleDomainExpand) return new Set([domain]);
                else next.add(domain);
                return next;
            });
            // Only clear the selection if we're collapsing the domain that contains the selected topic,
            // since that node will no longer be visible.
            setCurrentTopicSelected((current) => {
                if (!current) return null;
                const selectedDomain = (current.topicInfo as UserScopeInfo).ancestorName;
                const isCollapsing = expandedDomains.has(domain);
                // In single-expand mode, expanding a new domain implicitly collapses all others
                const implicitlyCollapsed =
                    singleDomainExpand && !isCollapsing && selectedDomain !== domain;
                return (isCollapsing && selectedDomain === domain) || implicitlyCollapsed
                    ? null
                    : current;
            });
            if (!skipFitView) {
                setTimeout(() => {
                    fitView({ duration: 500, padding: 0.15 });
                }, 80);
            }
        },
        [fitView, expandedDomains, singleDomainExpand]
    );

    const visibleNodes = useMemo(() => {
        const { positions, visibleIds } = computeLayout(nodes, edges, expandedDomains, domains);
        computedPositionsRef.current = positions;
        return nodes
            .filter((node) => visibleIds.has(node.id))
            .map((node) => {
                const pos = positions.get(node.id);
                const base = pos ? { ...node, position: pos } : node;
                if (node.type === "domain") {
                    return {
                        ...base,
                        data: {
                            ...base.data,
                            expanded: expandedDomains.has((node.data as any).domainKey),
                            setExpandedDomain: handleDomainExpand,
                        },
                    };
                }
                return base;
            });
    }, [nodes, edges, expandedDomains, handleDomainExpand]);

    const visibleEdges = useMemo(() => {
        const visibleIds = new Set(visibleNodes.map((n) => n.id));
        return computeVisibleEdges(edges as Edge[], visibleIds, expandedDomains);
    }, [edges, visibleNodes, expandedDomains]);

    const { isMobile } = useBreakPoints();
    const focusNode = useFocusNode();

    const updateTooltipPosition = (el: Element | null) => {
        if (!el) return;
        setCurrentTopicSelected((currentTopicSelected) => {
            if (!currentTopicSelected?.topicInfo) return null;
            return {
                topicInfo: currentTopicSelected.topicInfo,
                element: el,
            };
        });
    };

    const handleMove = () => {
        updateTooltipPosition(currentTopicSelected?.element ?? null);
    };

    useEffect(() => {
        GetUserTopicInfo()
            .then((result) => {
                return result.map((topicInfo: UserScopeInfo) => ({
                    id: topicInfo.scopeId?.toString(),
                    position: { x: 0, y: 0 },
                    data: {
                        label: topicInfo.scopeName,
                        objectInfo: topicInfo,
                        setCurrentTopicSelected,
                    },
                    type: topicInfo.mastered
                        ? "mastered"
                        : topicInfo.available
                          ? "unlocked"
                          : "locked",
                    width: topicNodeWidth,
                    height: topicNodeHeight,
                }));
            })
            .then((newNodes) =>
                GetTopicDependencies().then((result) => {
                    const topicEdges = result.map((dd: ScopeEdge, id: number) => ({
                        id: id.toString(),
                        source: dd.from.toString(),
                        target: dd.to.toString(),
                        weight: dd.weight,
                    }));

                    const domainOrdering = new Map<string, string>();
                    for (const edgeId in topicEdges) {
                        const edge = topicEdges[edgeId];

                        const from = newNodes.find((i) => i.id === edge.source);
                        const to = newNodes.find((i) => i.id === edge.target);

                        if (!(from && to)) {
                            continue;
                        }
                        const fromDomain = from.data.objectInfo.ancestorName;
                        const toDomain = to.data.objectInfo.ancestorName;

                        if (fromDomain !== toDomain) {
                            domainOrdering.set(fromDomain, toDomain);
                        }
                    }
                    const targets = new Set(domainOrdering.values());
                    const startDomain = [...domainOrdering.keys()].find(
                        (node) => !targets.has(node)
                    );

                    const domainsOrdered: string[] = [];

                    let current = startDomain;
                    while (current) {
                        domainsOrdered.push(current);
                        current = domainOrdering.get(current);
                    }
                    setDomains(domainsOrdered);
                    const domainColorMap = getDomainColorMap(domainsOrdered);

                    // Create one synthetic domain header node per top-level domain.
                    const masteredDomains = new Set(
                        domainsOrdered.filter((domain) => {
                            const domainTopics = newNodes.filter(
                                (n) => (n.data.objectInfo as any).ancestorName === domain
                            );
                            return (
                                domainTopics.length > 0 &&
                                domainTopics.every((n) => n.type === "mastered")
                            );
                        })
                    );

                    const syntheticDomainNodes = Object.keys(domainColorMap).map((domain) => ({
                        id: `domain-${domain}`,
                        position: { x: 0, y: 0 },
                        data: {
                            label: domain,
                            domainKey: domain,
                            domainColor: domainColorMap[domain],
                            expanded: false,
                            allMastered: masteredDomains.has(domain),
                            setExpandedDomain: handleDomainExpand,
                        },
                        type: "domain" as const,
                        width: domainNodeWidth,
                        height: domainNodeHeight,
                    }));

                    const allNodes = [...newNodes, ...syntheticDomainNodes];
                    const newEdges = buildEdges(topicEdges, allNodes, domainsOrdered);

                    // Layout with metro design
                    const { nodes: layoutedNodes, edges: layoutedEdges } = isMobile
                        ? layoutMetroMobile(allNodes, newEdges)
                        : layoutMetro(allNodes, newEdges);

                    setNodes(layoutedNodes);
                    setEdges(layoutedEdges);

                    // Set color map for edges based on source node domain
                    // Also set node status map to determine edge color brightness
                    const colorMap = new Map<string, string>();
                    const nodeStatusMap = new Map<string, string>();
                    allNodes.forEach((node) => {
                        // Domain header nodes don't participate in edge coloring
                        if (node.type === "domain") return;
                        const parentDomain = (node.data as any).objectInfo.ancestorName;
                        colorMap.set(
                            node.id ?? "",
                            domainColorMap[parentDomain] ?? "var(--color-edge-default)"
                        );
                        nodeStatusMap.set(node.id ?? "", node.type); // "mastered", "unlocked", or "locked"
                    });
                    // Set color map state to trigger re-render with correct edge colors
                    setNodeColorMap(colorMap);
                    setNodeStatusMap(nodeStatusMap);

                    setShouldSuggestTopic(true);
                })
            );
    }, []);

    useEffect(() => {
        if (!shouldSuggestTopic) {
            return;
        }

        GetNextSuggestedTopic()
            .then((suggestedTopic: TopicLabel | null) => {
                if (!suggestedTopic) return;
                // Mark the suggested node (styling only — position comes from visibleNodes)
                setNodes((nodes) =>
                    nodes.map((node) => ({
                        ...node,
                        data: {
                            ...node.data,
                            suggested: node.id === suggestedTopic.scopeId.toString(),
                        },
                    }))
                );

                // Focus on the suggested topic
                const suggestedNode = nodes.find((n) => n.id === suggestedTopic.scopeId.toString());
                if (suggestedNode) {
                    const domain = (suggestedNode.data as any).objectInfo?.ancestorName;
                    if (domain) setExpandedDomains((prev) => new Set([...prev, domain]));

                    // Small delay to ensure nodes are rendered and positions are committed
                    setTimeout(() => {
                        const nodeElement = document.querySelector(
                            `[data-id="${suggestedNode.id}"] .base-node`
                        ) as Element | null;

                        if (nodeElement) {
                            setCurrentTopicSelected({
                                topicInfo: (suggestedNode.data as any).objectInfo,
                                element: nodeElement,
                            });
                        }

                        // Use the dynamically computed position (accounts for domain shift)
                        const pos = computedPositionsRef.current.get(suggestedNode.id);
                        if (pos) focusNode(pos.x, pos.y);

                        // Focus the container to enable keyboard navigation immediately after focusing the node
                        containerRef.current?.focus();
                    }, 400); // Delay should be long enough to ensure nodes are rendered and positioned, but can be adjusted based on performance and UX.
                }
            })
            .catch((error) => {
                console.error(error);
            });
    }, [shouldSuggestTopic, setNodes, refresh]);

    // Keyboard navigation handler to move focus between nodes using arrow keys
    const handleKeyDown = useCallback(
        (event: React.KeyboardEvent) => {
            const focusedId = currentTopicSelected?.topicInfo.scopeId?.toString();
            if (
                !focusedId ||
                !["ArrowUp", "ArrowDown", "ArrowLeft", "ArrowRight"].includes(event.key)
            )
                return;

            event.preventDefault();

            // Compute positions for ALL topic nodes (collapsed or not) using a fully-expanded
            // layout so all positions share the same coordinate system for direction checks.
            const { positions: allPositions } = computeLayout(
                nodes,
                edges,
                new Set(domains),
                domains
            );

            const currentPos = allPositions.get(focusedId);
            if (!currentPos) return;

            const allTopicNodes = nodes.filter((n) => n.type !== "domain");

            const candidates = allTopicNodes.filter((n) => {
                if (n.id === focusedId) return false;
                const pos = allPositions.get(n.id);
                if (!pos) return false;
                const dx = pos.x - currentPos.x;
                const dy = pos.y - currentPos.y;

                switch (event.key) {
                    case "ArrowRight":
                        return dx > 20;
                    case "ArrowLeft":
                        return dx < -20;
                    case "ArrowDown":
                        return dy > 20;
                    case "ArrowUp":
                        return dy < -20;
                    default:
                        return false;
                }
            });

            if (candidates.length === 0) return;

            // Weight distance based on direction — prioritize the primary axis
            const isHorizontal = event.key === "ArrowRight" || event.key === "ArrowLeft";
            const nearest = candidates.reduce((closest, node) => {
                const pos = allPositions.get(node.id)!;
                const cpos = allPositions.get(closest.id)!;
                const dx = pos.x - currentPos.x;
                const dy = pos.y - currentPos.y;
                const cdx = cpos.x - currentPos.x;
                const cdy = cpos.y - currentPos.y;

                const nodeConnected = edges.some(
                    (e) =>
                        (e.source === focusedId && e.target === node.id) ||
                        (e.target === focusedId && e.source === node.id)
                );
                const closestConnected = edges.some(
                    (e) =>
                        (e.source === focusedId && e.target === closest.id) ||
                        (e.target === focusedId && e.source === closest.id)
                );

                // Prefer connected nodes
                if (nodeConnected && !closestConnected) return node;
                if (!nodeConnected && closestConnected) return closest;

                // Among same connection status, use weighted distance
                const distCurrent = isHorizontal
                    ? Math.abs(dx) + Math.abs(dy) * 3
                    : Math.abs(dy) + Math.abs(dx) * 3;
                const distClosest = isHorizontal
                    ? Math.abs(cdx) + Math.abs(cdy) * 3
                    : Math.abs(cdy) + Math.abs(cdx) * 3;

                return distCurrent < distClosest ? node : closest;
            });

            const nearestDomain = (nearest.data as any).objectInfo?.ansestorName;
            const isInCollapsedDomain = nearestDomain && !expandedDomains.has(nearestDomain);

            if (isInCollapsedDomain) {
                // Clear tooltip immediately — the layout shifts when the domain expands,
                // making any currently-shown tooltip position stale.
                setCurrentTopicSelected(null);
                // Expand the domain without fitView — focusNode below handles the viewport.
                handleDomainExpand(nearestDomain, true);
                // Start the viewport animation once the DOM has settled (~100ms).
                // Show the tooltip only after the animation completes so it appears on a
                // centered, stationary node rather than sliding in from off-screen.
                const animationDuration = 500;
                setTimeout(() => {
                    const pos = computedPositionsRef.current.get(nearest.id);
                    if (pos) focusNode(pos.x, pos.y, undefined, animationDuration);
                }, 100);
                setTimeout(() => {
                    const nodeElement = document.querySelector(
                        `[data-id="${nearest.id}"] .base-node`
                    ) as Element | null;
                    if (nodeElement) {
                        setCurrentTopicSelected({
                            topicInfo: (nearest.data as any).objectInfo,
                            element: nodeElement,
                        });
                    }
                }, 100 + animationDuration);
                return;
            }

            // Node is already visible — use its actual rendered position
            const nodeElement = document.querySelector(
                `[data-id="${nearest.id}"] .base-node`
            ) as Element | null;

            if (nodeElement) {
                setCurrentTopicSelected({
                    topicInfo: (nearest.data as any).objectInfo,
                    element: nodeElement,
                });
            }

            const renderedPos = computedPositionsRef.current.get(nearest.id);
            if (renderedPos) focusNode(renderedPos.x, renderedPos.y);
        },
        [currentTopicSelected, nodes, edges, expandedDomains, handleDomainExpand]
    );

    return (
        <div
            style={{
                height: "100%",
                width: "100%",
                position: "relative",
                zIndex: 0,
                overflow: "hidden",
            }}
            ref={containerRef}
            onKeyDown={handleKeyDown}
            tabIndex={0}
        >
            <ReactFlow
                nodes={visibleNodes}
                edgeTypes={edgeTypes}
                edges={visibleEdges.map((edge) => {
                    // Determine edge color and animation based on the status of the source and target nodes
                    const sourceStatus = nodeStatusMap.get(edge.source);
                    const targetStatus = nodeStatusMap.get(edge.target);
                    const domainBase = nodeColorMap.get(edge.source) ?? "--color-edge-default";
                    const isBright =
                        sourceStatus === "mastered" &&
                        (targetStatus === "unlocked" ||
                            targetStatus === "mastered" ||
                            edge.target.startsWith("domain-"));
                    const suffix = isBright ? "-edge-mastered" : "-edge-locked";
                    const edgeColor =
                        domainBase === "--color-edge-default"
                            ? domainBase
                            : `${domainBase}${suffix}`;

                    return {
                        ...edge,
                        type: "edge",
                        animated: isBright,
                        style: {
                            strokeWidth: 12,
                            stroke: `var(${edgeColor})`,
                        },
                    };
                })}
                style={{ background: "var(--color-surface-graph-default)" }}
                nodeTypes={nodeTypes}
                onMove={handleMove}
                onMoveEnd={handleMove}
                panOnDrag={!tourActive}
                zoomOnScroll={!tourActive}
                zoomOnPinch={!tourActive}
                zoomOnDoubleClick={!tourActive}
                onPaneClick={() => setCurrentTopicSelected(null)}
                minZoom={0.4}
                maxZoom={2}
                fitView
                nodesDraggable={false}
                nodesConnectable={false}
                proOptions={{ hideAttribution: true }}
                translateExtent={
                    isMobile
                        ? [
                              [-1000, -1250],
                              [6500, 1250],
                          ]
                        : [
                              [-750, -500],
                              [5000, 1250],
                          ]
                }
            >
                {currentTopicSelected && <Tooltip selectedTopic={currentTopicSelected} />}
                {/* MiniMap renders a small overview of the full graph in the bottom-left corner.
                    nodeColor assigns each node its domain color using the same CSS variables as the tree.
                    The -mastered/-locked suffix controls brightness: brighter for mastered, dimmer for locked/unlocked.
                    ReactFlow's MiniMap applies the returned string via style.fill, so CSS var() works here. */}
                <MiniMap
                    position="bottom-left"
                    zoomable
                    nodeColor={(node) => {
                        if (node.type === "domain") {
                            // Use -mastered color when all topics in the domain are completed, -locked otherwise
                            const cssVar = domainColorMap[(node.data as any).domainKey];
                            const suffix = (node.data as any).allMastered ? "-mastered" : "-locked";
                            return cssVar
                                ? `var(${cssVar}${suffix})`
                                : "var(--color-minimap-node-default)";
                        }
                        // Topic nodes: look up their domain via the topic path, then pick brightness by status
                        const domain = (node.data as any)?.objectInfo?.ancestorName;
                        const cssVar = domain ? domainColorMap[domain] : null;
                        const suffix = node.type === "mastered" ? "-mastered" : "-locked";
                        return cssVar
                            ? `var(${cssVar}${suffix})`
                            : "var(--color-minimap-node-default)";
                    }}
                    nodeStrokeWidth={16}
                    nodeStrokeColor={(node) => {
                        if (node.type === "domain") {
                            const cssVar = domainColorMap[(node.data as any).domainKey];
                            const suffix = (node.data as any).allMastered
                                ? "-mastered-border"
                                : "-locked-border";
                            return cssVar
                                ? `var(${cssVar}${suffix})`
                                : "var(--color-minimap-node-default)";
                        }
                        const domain = (node.data as any)?.objectInfo?.ancestorName;
                        const cssVar = domain ? domainColorMap[domain] : null;
                        const suffix =
                            node.type === "mastered" ? "-mastered-border" : "-locked-border";
                        return cssVar
                            ? `var(${cssVar}${suffix})`
                            : "var(--color-minimap-node-default)";
                    }}
                    nodeBorderRadius={16}
                    bgColor="var(--color-minimap-bg)"
                    maskColor="var(--color-minimap-mask)"
                    onNodeClick={(_, node) => {
                        if (node.type === "domain")
                            handleDomainExpand((node.data as any).domainKey);
                    }}
                />
                <Legend />
            </ReactFlow>
            {expandedDomains.size > 0 && (
                <div className="absolute top-4 right-4 z-10 flex flex-col gap-2">
                    {[...expandedDomains].map((domain) => (
                        <button
                            key={domain}
                            onClick={() => handleDomainExpand(domain)}
                            className="flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-bold cursor-pointer border-2 text-text-dark"
                            style={{
                                backgroundColor: `var(${domainColorMap[domain]}-locked)`,
                                borderColor: `var(${domainColorMap[domain]}-mastered-border)`,
                            }}
                        >
                            <ArrowsPointingInIcon className="w-3 h-3 shrink-0" />
                            {domain}
                        </button>
                    ))}
                </div>
            )}
        </div>
    );
}
