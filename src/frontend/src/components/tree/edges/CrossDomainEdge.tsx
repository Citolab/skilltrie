/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */
import { type EdgeProps } from "@xyflow/react";

/**
 * Edge used for connections that bridge across domain boundaries
 * (last topic -> next domain, domain -> first topic).
 *
 * Rather than a straight/diagonal line that would visually pass through
 * intermediate nodes, this draws a cubic bezier that arcs above the node
 * cluster, making it unambiguous that the path is not connected to anything
 * in between.
 */
export default function CrossDomainEdge({
    sourceX, sourceY, targetX, targetY, style, animated,
}: EdgeProps) {
    const CLEARANCE = 40;
    const CORNER = 40;
    const dy = Math.abs(sourceY - targetY);

    // When source and target are at the same height, a straight line is sufficient.
    // Only arc above with angled corners when the y positions differ.
    const path = dy < CORNER
        ? `M ${sourceX},${sourceY} L ${targetX},${targetY}`
        : [
            `M ${sourceX},${sourceY}`,
            `L ${sourceX},${Math.min(sourceY, targetY) - CLEARANCE + CORNER}`,   // up to top-left corner
            `L ${sourceX + CORNER},${Math.min(sourceY, targetY) - CLEARANCE}`,   // 45° diagonal out
            `L ${targetX - CORNER},${Math.min(sourceY, targetY) - CLEARANCE}`,   // horizontal at peak
            `L ${targetX},${Math.min(sourceY, targetY) - CLEARANCE + CORNER}`,   // 45° diagonal in
            `L ${targetX},${targetY}`,                                            // down to target
        ].join(" ");

    return (
        <path
            d={path}
            style={{
                ...style,
                fill: "none",
                strokeDasharray: animated ? "6 6" : "none",
                animation: animated ? "edge-flow 1s linear infinite" : "none",
            }}
        />
    );
}
