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

type UnlockedNode = Node<NodeData, "unlocked">;

export default function UnlockedNode({ data }: NodeProps<UnlockedNode>) {
    return <BaseNode data={data} className={"bg-slate-100 border-4 border-primary-light"} />;
}
