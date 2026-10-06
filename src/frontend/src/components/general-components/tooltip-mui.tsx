/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import type { CSSProperties } from "react";
import TooltipMui from '@mui/material/Tooltip';
import type { TooltipProps as TooltipMuiProps } from '@mui/material/Tooltip';
import type { Placement } from '@popperjs/core';
 
export type TooltipProps = TooltipMuiProps & {
    /** Background color. */
    backgroundColor?: CSSProperties["color"];
    /** Border size. */
    borderSize?: number;
    /** Border style. */
    borderStyle?: CSSProperties["borderStyle"];
    /** Border color. */
    borderColor?: CSSProperties["color"];
    /** Width of the tooltip. */
    width?: number;
    /** Height of the tooltip. */
    height?: number;
 }

export default function Tooltip({
    backgroundColor = "var(--color-background-light)",
    borderSize = 1,
    borderStyle = "solid",
    borderColor = "var(--color-primary)",
    width,
    height,
    ...props
}: TooltipProps) {
    function getFallbackPlacements(placement: Placement | undefined) {
        switch (placement) {
            case "top":             return ["bottom"        , "right"   , "left"];
            case "bottom":          return ["top"           , "right"   , "left"];
            case "right":           return ["left"          , "top"     , "bottom"];
            case "left":            return ["right"         , "top"     , "bottom"];
            case "top-start":       return ["bottom-start"  , "left"    , "right"];
            case "bottom-start":    return ["top-start"     , "left"    , "right"];
            case "right-start":     return ["left-start"    , "bottom"  , "top"];
            case "left-start":      return ["right-start"   , "bottom"  , "top"];
            case "top-end":         return ["bottom"        , "right"   , "left"];
            case "bottom-end":      return ["top"           , "right"   , "left"];
            case "right-end":       return ["left"          , "top"     , "bottom"];
            case "left-end":        return ["right"         , "top"     , "bottom"];
            default:                return undefined;
        }
    }

    return <TooltipMui
        describeChild
        arrow
        disableFocusListener
        disableHoverListener
        disableTouchListener
        slotProps={{
            tooltip: {
                sx: {
                    // Apply the background color
                    bgcolor: backgroundColor,
                    // Apply text colour
                    color: "var(--text-dark)",
                    // Apply shadow
                    boxShadow: 10,
                    // Apply shape
                    borderRadius: 2,
                    // Apply border
                    border: `${borderSize}px ${borderStyle} ${borderColor}`,
                    // Apply size
                    width: width,
                    height: height
                },
                // Caller's tooltip is merged last so it can override anything above
                ...props?.slotProps?.tooltip
            },
            arrow: {
                sx: {
                    // Apply the background color
                    color: backgroundColor,
                    // Apply border
                    "&:before": {
                        border: `${borderSize}px ${borderStyle} ${borderColor}`
                    }
                },
                // Caller's arrow is merged last so it can override anything above
                ...props?.slotProps?.arrow
            },
            popper: {
                modifiers: [
                    {
                        name: "flip",
                        enabled: true,
                        options: {
                            // fallbackPlacements: fallbackPlacements[props.placement]
                            fallbackPlacements: getFallbackPlacements(props.placement)
                        }
                    },
                    {
                        name: "preventOverflow",
                        enabled: true,
                        options: {
                            boundary: "window"
                        }
                    }
                ],
                // Caller's arrow is merged last so it can override anything above
                ...props?.slotProps?.popper
            }
        }}
        {...props}
    />
}