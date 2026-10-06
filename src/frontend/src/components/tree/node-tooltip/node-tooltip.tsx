/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import type { ComponentType, SVGProps } from "react";
import { useViewport } from "@xyflow/react";
import type { SelectedTopic } from "../tree.tsx";
import { GetRandomLevel } from "../../../api/level.ts";
import { toast } from "react-toastify";
import { useLevelContext } from "../../../contexts/level-context.tsx";
import { useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { getTooltipStyling } from "./tooltip-styling.ts";
import { usePostHog } from "posthog-js/react";

interface TooltipProps {
    selectedTopic: SelectedTopic;
}

export interface NodeTooltipStyling {
    tooltipColor: string;
    buttonColor: string;
    hoverButtonColor: string;
    mainTextColor: string;
    subTextColor: string;
    buttonText?: string;
    buttonDisabled?: boolean;
    icon?: ComponentType<SVGProps<SVGSVGElement>>;
    iconColor?: string;
}

export const tooltipHeight = 170; // quick estimate (can improve later)

export const Tooltip = ({ selectedTopic }: TooltipProps) => {
    const ARROW_SIZE = 20;
    const { element, topicInfo } = selectedTopic;
    const { levelResponse, setLevelResponse } = useLevelContext();
    const { zoom } = useViewport();
    const navigate = useNavigate();
    const posthog = usePostHog();
    const rect = element.getBoundingClientRect();
    const styling: NodeTooltipStyling = getTooltipStyling(topicInfo);

    const handleStart = () => {
        posthog.capture("level_started", {
            topic_id: topicInfo.scopeId,
            topic_name: topicInfo.scopeName,
        });
        GetRandomLevel(topicInfo.scopeId)
            .then(setLevelResponse)
            .catch((error) => {
                toast.error("Could not generate random level... \nPlease try again later");
                console.error(error);
            });
    };

    useEffect(() => {
        if (levelResponse != null) navigate("/qti", { replace: true });
    }, [levelResponse]);

    const containerEl = document.querySelector(".react-flow") as HTMLElement;
    const containerRect = containerEl.getBoundingClientRect();

    const relativeLeft = rect.left - containerRect.left + rect.width / 2;
    const relativeTop = rect.top - containerRect.top;

    const tooltipTooHigh = relativeTop < tooltipHeight;

    const StylingIcon = styling.icon;

    return (
        <>
            <style>{`
                .node-tooltip .play-button:hover {
                    background-color: ${styling.hoverButtonColor}!important;
                }`}</style>
            <div
                className={`node-tooltip flex flex-col gap-0 rounded-lg text-white p-4 `}
                style={{
                    backgroundColor: styling.tooltipColor,
                    position: "absolute",
                    left: relativeLeft,
                    top: tooltipTooHigh ? relativeTop + rect.height : relativeTop,
                    width: Math.min(300, window.innerWidth - 32),
                    height: Math.min((rect.height / zoom) * 5, 150),
                    zIndex: 5,
                    transform: tooltipTooHigh ? "translate(-50%, 10%)" : "translate(-50%, -110%)",
                }}
            >
                <p
                    className="flex-[1] text-sm text-gray-700"
                    style={{
                        color: styling.subTextColor,
                    }}
                >
                    {topicInfo.ancestorName}
                </p>
                <p
                    className="flex-[3] text-xl font-bold"
                    style={{
                        color: styling.mainTextColor,
                    }}
                >
                    {topicInfo.scopeName}
                </p>
                <div className="flex items-center gap-2 flex-[2]">
                    <span className="flex items-center">
                        {StylingIcon && (
                            <StylingIcon className="w-5 h-5" style={{ color: styling.iconColor }} />
                        )}
                    </span>
                    <p className="text-sm text-gray-300" style={{ color: styling.subTextColor }}>
                        {topicInfo.mastered
                            ? "Mastered"
                            : !topicInfo.available
                              ? "Locked"
                              : "Not mastered"}
                    </p>
                </div>
                <button
                    style={{
                        backgroundColor: styling.buttonColor,
                        color: styling.mainTextColor,
                    }}
                    className={`flex-[1] play-button w-full rounded-3xl text-l cursor-pointer hover:shadow-xl active:translate-y-0.5 transition-all duration-50`}
                    onClick={styling.buttonDisabled == true ? () => {} : handleStart}
                >
                    {styling.buttonText ?? "Start Level"}
                </button>

                <div
                    style={{
                        position: "absolute",
                        left: "50%",
                        transform: "translateX(-50%)",
                        width: 0,
                        height: 0,
                        ...(tooltipTooHigh
                            ? {
                                  top: -ARROW_SIZE,
                                  borderLeft: `${ARROW_SIZE}px solid transparent`,
                                  borderRight: `${ARROW_SIZE}px solid transparent`,
                                  borderBottom: `${ARROW_SIZE}px solid ${styling.tooltipColor}`,
                              }
                            : {
                                  bottom: -ARROW_SIZE + 2,
                                  borderLeft: `${ARROW_SIZE}px solid transparent`,
                                  borderRight: `${ARROW_SIZE}px solid transparent`,
                                  borderTop: `${ARROW_SIZE}px solid ${styling.tooltipColor}`,
                              }),
                    }}
                />
            </div>
        </>
    );
};
