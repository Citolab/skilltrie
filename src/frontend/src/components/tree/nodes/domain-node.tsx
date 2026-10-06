/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */
import { Handle, Position } from "@xyflow/react";
import { type Node, type NodeProps } from "@xyflow/react";
import "../../style/tree.css";
import { domainNodeWidth, domainNodeHeight } from "../../../utils/tree/layoutMetro.tsx";

export type DomainNodeData = {
    label: string;
    domainKey: string;
    domainColor: string;
    expanded: boolean;
    allMastered: boolean;
    setExpandedDomain: (domain: string) => void;
};

type DomainNodeType = Node<DomainNodeData, "domain">;

const centeredHandle = {
    left: "50%",
    top: "50%",
    transform: "translate(-50%, -50%)",
};

export default function DomainNode({ data }: NodeProps<DomainNodeType>) {
    const handleClick = () => {
        data.setExpandedDomain(data.domainKey);
    };

    return (
        <div
            className="base-node border-4 cursor-pointer select-none"
            style={{
                width: domainNodeWidth,
                height: domainNodeHeight,
                backgroundColor: data.allMastered
                    ? `var(${data.domainColor}-mastered)`
                    : `var(${data.domainColor}-locked)`,
                borderColor: data.expanded || data.allMastered
                    ? `var(${data.domainColor}-mastered-border)`
                    : `var(${data.domainColor}-locked-border)`,
            }}
            onClick={handleClick}
        >
            <p className="font-bold text-2xl leading-tight px-2">{data.label}</p>
            <Handle type="source" position={Position.Right} style={centeredHandle} />
            <Handle type="target" position={Position.Left} style={centeredHandle} />
        </div>
    );
}
