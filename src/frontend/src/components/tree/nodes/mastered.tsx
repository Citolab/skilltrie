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

type MasteredNode = Node<NodeData, "mastered">;

export default function MasteredNode({ data }: NodeProps<MasteredNode>) {
    return <BaseNode data={data} className={"border-4 border-emerald-400 bg-slate-100"} />;
}
