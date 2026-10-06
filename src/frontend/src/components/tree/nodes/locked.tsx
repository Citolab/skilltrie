/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { type Node, type NodeProps } from "@xyflow/react";
import "@xyflow/react/dist/style.css";
import "../../style/tree.css";
import { BaseNode, type NodeData } from "./base-node.tsx";
import { lockIcon } from "../../sidebar/sidebarlogos";

type LockedNode = Node<NodeData, "mastered">;

export default function LockedNode({ data }: NodeProps<LockedNode>) {
    return (
        <div style={{ position: "relative" }}>
            <span className="absolute top-0 right-0 z-10 text-gray-800">{lockIcon}</span>
            <BaseNode data={data} className="bg-gray-300  opacity-80" />
        </div>
    );
}
