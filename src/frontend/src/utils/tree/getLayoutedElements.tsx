/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import dagre from "@dagrejs/dagre";
import { useBreakPoints } from "../../hooks/useBreakPoints.tsx";

const dagreGraph = new dagre.graphlib.Graph().setDefaultEdgeLabel(() => ({}));

//Create the nodes
const nodeWidth = 250;
const nodeHeight = 150;

export const getLayoutedElements = (nodes: any, edges: any, isMobile: boolean) => {
    var direction = "LR"
    direction = isMobile ? "TB" : direction;
    
    dagreGraph.setGraph({ rankdir: direction,
                          ranker: "tight-tree",
                          nodesep: isMobile ? 30 : 80,
                          ranksep: isMobile ? 80 : 150 });

    nodes.forEach((node: any) => {
        dagreGraph.setNode(node.id, { width: nodeWidth, height: nodeHeight });
    });

    edges.forEach((edge: any) => {
        dagreGraph.setEdge(edge.source, edge.target, { weight: edge.weight ?? 1 });
    });

    dagre.layout(dagreGraph);

    const newNodes = nodes.map((node: any) => {
        const nodeWithPosition = dagreGraph.node(node.id);
        return {
            ...node,
            targetPosition: !isMobile ? "left" : "top",
            sourcePosition: !isMobile ? "right" : "bottom",
            // We are shifting the dagre node position (anchor=center center) to the top left
            // so it matches the React Flow node anchor point (top left).
            position: {
                x: nodeWithPosition.x - nodeWidth / 2,
                y: nodeWithPosition.y - nodeHeight / 2,
            },
            data: {
                ...node.data,
                positionX: nodeWithPosition.x - nodeWidth / 2,
                positionY: nodeWithPosition.y - nodeHeight / 2,
            },
        };
    });

    return { nodes: newNodes, edges };
};
