/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { useState } from "react";
import { Box, IconButton } from "@mui/material";
import { ChevronRightIcon, ChevronLeftIcon } from "@heroicons/react/24/solid";

export function CycleDisplay<T extends string>({
    options,
    value,
    onChange,
    renderItem,
    className,
}: {
    options: T[];
    value: T;
    onChange: (value: T) => void;
    renderItem: (value: T) => React.ReactNode;
    className?: string;
}) {
    const [index, setIndex] = useState<number>(options.indexOf(value));

    function changeIndex(newIndex: number) {
        setIndex(newIndex);
        onChange(options[newIndex]);
    }

    return (
        <div>
            <Box className={`flex gap-1 items-center ${className}`}>
                <IconButton
                    aria-label="Previous item"
                    onClick={() => changeIndex((index - 1 + options.length) % options.length)}
                >
                    <ChevronLeftIcon className="w-7" />
                </IconButton>
                {renderItem(options[index])}
                <IconButton
                    aria-label="Next item"
                    onClick={() => changeIndex((index + 1) % options.length)}
                >
                    <ChevronRightIcon className="w-7" />
                </IconButton>
            </Box>
            <p className="noteText text-center">
                <span>{index + 1}</span> of <span>{options.length}</span>
            </p>
        </div>
    );
}
