/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */
import { type EdgeProps } from "@xyflow/react";

function getMetroPath(sourceX: number, sourceY: number, targetX: number, targetY: number): string {
    const dx = targetX - sourceX;
    const dy = targetY - sourceY;
    const absDy = Math.abs(dy);

    if (absDy < 10) {
        return `M ${sourceX},${sourceY} L ${targetX},${targetY}`;
    }

    const padding = 120; // horizontal straight segment near nodes
    const diagonalLength = absDy;
    const availableX = Math.abs(dx) - padding * 2;
    const diagonalX = Math.min(diagonalLength, availableX);
    const startDiagX = sourceX + padding * Math.sign(dx);
    const endDiagX = startDiagX + diagonalX * Math.sign(dx);

    return `M ${sourceX},${sourceY} 
            L ${startDiagX},${sourceY} 
            L ${endDiagX},${targetY} 
            L ${targetX},${targetY}`;
}

export default function MetroEdge({
    sourceX, sourceY, targetX, targetY, style, animated,
}: EdgeProps) {
    const edgePath = getMetroPath(sourceX, sourceY, targetX, targetY);

    return (
        <path
            d={edgePath}
            style={{
                ...style,
                fill: "none",
                strokeDasharray: animated ? "6 6" : "none",
                animation: animated ? "edge-flow 1s linear infinite" : "none",
            }}
        />
    );
}