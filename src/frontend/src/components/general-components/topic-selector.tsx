/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { useEffect, useState } from "react";
import type { TopicLabel } from "../../types/scope.ts";
import { toast } from "react-toastify";
import Select, { type SelectComponentsConfig, type StylesConfig } from "react-select";
import type { GroupBase } from "react-select";

interface TopicSelectorProps {
    /** Called with the chosen topic (or null when cleared). */
    setSelectedTopic: (selected: TopicLabel | null) => void;
    /** Loads the available topics; invoked once on mount. */
    getTopics: () => Promise<TopicLabel[]>;
    /** Extra class names applied to the react-select container. */
    className?: string;
    /** react-select style overrides (e.g. to match surrounding inputs). */
    styles?: StylesConfig<TopicLabel, false>;
    /** react-select component overrides (e.g. a custom dropdown indicator). */
    components?: Partial<SelectComponentsConfig<TopicLabel, false, GroupBase<TopicLabel>>>;
}

/**
 * Searchable single-select dropdown for picking a topic.
 *
 * Fetches the topic list on mount via {@link TopicSelectorProps.getTopics} and renders
 * nothing until it resolves (toasting on failure). `styles` and `components` are
 * forwarded to react-select so callers can restyle it without forking this component.
 */
export function TopicSelector(props: TopicSelectorProps) {
    const [topics, setTopics] = useState<TopicLabel[] | null>(null);

    useEffect(() => {
        props
            .getTopics()
            .then(setTopics)
            .catch((error) => {
                toast.error("Could not retrieve topics...");
                console.error(error);
            });
    }, []);

    return (
        topics && (
            <Select
                options={topics}
                getOptionValue={(t) => String(t.scopeId)}
                getOptionLabel={(t) => t.scopeName}
                onChange={(option) => props.setSelectedTopic(option)}
                className={`cursor-pointer ${props.className}`}
                classNamePrefix="select"
                placeholder="Search or select topics..."
                styles={props.styles}
                components={props.components}
            />
        )
    );
}
