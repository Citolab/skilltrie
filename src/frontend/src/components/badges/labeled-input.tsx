/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import type { ReactNode } from "react";
import { IconButton, Tooltip } from "@mui/material";
import InfoOutlinedIcon from "@mui/icons-material/InfoOutlined";

/**
 * Wraps a form control with a label above it and an optional (i) tooltip.
 *
 * @param label - Text shown above the control.
 * @param info - Optional helper text rendered in an {@link InfoTooltip}.
 * @param children - The input/control to label.
 */
export const LabeledField = ({
    label,
    info,
    children,
}: {
    label: string;
    info?: string;
    children: ReactNode;
}) => (
    <div className="flex flex-col gap-1.5">
        <div className="flex items-center gap-1">
            <span className="text-sm font-medium font-label">{label}</span>
            {info && <InfoTooltip text={info} />}
        </div>
        {children}
    </div>
);

/** A small (i) icon that reveals descriptive `text` in a tooltip on hover/focus. */
export const InfoTooltip = ({ text }: { text: string }) => (
    <Tooltip title={text} arrow placement="top">
        <IconButton
            size="small"
            tabIndex={-1}
            sx={{ p: 0, color: "var(--color-nav-text-LD)" }}
            aria-label="More information"
        >
            <InfoOutlinedIcon fontSize="small" />
        </IconButton>
    </Tooltip>
);
