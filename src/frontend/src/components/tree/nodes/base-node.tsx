/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { Handle, Position, useReactFlow, useNodeId } from "@xyflow/react";
import { topicNodeWidth, topicNodeHeight } from "../../../utils/tree/layoutMetro.tsx";
import type { UserScopeInfo } from "../../../types/scope.ts";
import { type Dispatch, type RefObject, type SetStateAction, useCallback, useRef } from "react";
import type { SelectedTopic } from "../tree.tsx";
import { useBreakPoints } from "../../../hooks/useBreakPoints.tsx";
import { motion } from "motion/react";
import { StarIcon } from "@heroicons/react/24/solid";
import { useABTest } from "../../../hooks/ab-testing.ts";
import { fancyTopicSuggestionAbTest } from "../../../utils/ab-tests/fancy-topic-suggestion-ab-test.tsx";

export function useFocusNode() {
    const { setCenter } = useReactFlow();
    const { isMobile, isLandscape } = useBreakPoints();

    return useCallback(
        (x: number, y: number, zoom = 1.25, duration = 500) => {
            const adjustedZoom = isLandscape ? 0.75 : zoom;
            const mobileOffsetX = isMobile ? window.innerWidth * -0.1 : 0;
            const offsetMultiplierY = isLandscape ? -0.45 : isMobile ? -0.2 : -0.15;
            const mobileOffsetY = window.innerHeight * offsetMultiplierY;
            setCenter(
                x + topicNodeWidth / 2 + mobileOffsetX,
                y + topicNodeHeight / 2 + mobileOffsetY,
                { zoom: adjustedZoom, duration }
            );
        },
        [setCenter, isMobile, isLandscape]
    );
}

export type NodeData = {
    label: string;
    objectInfo: UserScopeInfo;
    setCurrentTopicSelected: Dispatch<SetStateAction<SelectedTopic | null>>;
    suggested: boolean;
};

interface BaseNodeProps {
    data: NodeData;
    className: string;
}

const centeredHandle = {
    left: "50%",
    top: "50%",
    transform: "translate(-50%, -50%)",
};

export const BaseNode = ({ data, ...props }: BaseNodeProps) => {
    // @ts-ignore initial value will be updated to node div
    const nodeRef: RefObject<HTMLDivElement> = useRef(null);
    const { getNode } = useReactFlow();
    const nodeId = useNodeId();
    const focusNode = useFocusNode();

    const { isMobile } = useBreakPoints();
    const sourcePosition = isMobile ? Position.Bottom : Position.Left;
    const targetPosition = isMobile ? Position.Top : Position.Right;

    const selectNode = () => {
        data.setCurrentTopicSelected({
            topicInfo: data.objectInfo,
            element: nodeRef.current,
        });

        // Use the node's actual current position from ReactFlow so centering works
        // even after the layout shifts due to domain expansion.
        const node = nodeId ? getNode(nodeId) : null;
        focusNode(node?.position.x ?? 0, node?.position.y ?? 0);
    };

    const doFancyTopicSuggestion = useABTest(fancyTopicSuggestionAbTest);

    return (
        <motion.div
            animate={doFancyTopicSuggestion && data.suggested ? { scale: [1, 1.05, 1] } : {}}
            transition={
                doFancyTopicSuggestion && data.suggested
                    ? { duration: 1.5, repeat: Infinity, ease: "easeInOut" }
                    : {}
            }
            className={`overflow-visible base-node ${props.className} 
                ${data.suggested && "base-node-border--suggested"} shadow-lg`}
            onClick={selectNode}
            ref={nodeRef}
            style={{ position: "relative", width: topicNodeWidth, height: topicNodeHeight }}
        >
            <p className="absolute top-2 left-1/2 -translate-x-1/2 text-xs whitespace-nowrap">
                {data.objectInfo.ancestorName}
            </p>
            <p className="font-bold">{data.label}</p>
            {doFancyTopicSuggestion && data.suggested && (
                <span className="absolute bottom-1 left-1/2 -translate-x-1/2 bg-primary text-white text-xs px-2 py-0.5 rounded-full shadow flex items-center gap-1">
                    <StarIcon className="w-3 h-3" />
                    Recommended
                </span>
            )}

            {/* Handles are used for connecting nodes with edges
                We style them to be hidden and centrally positioned */}
            <Handle type="source" position={sourcePosition} style={centeredHandle} />
            <Handle type="target" position={targetPosition} style={centeredHandle} />
        </motion.div>
    );
};
