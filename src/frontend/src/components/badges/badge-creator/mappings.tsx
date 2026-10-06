/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

// Used for displaying more user-friendly names than the class names returned from the backend
import type { DropdownIndicatorProps } from "react-select";
import type { TopicLabel } from "@/types/topic.ts";
import ExpandMoreIcon from "@mui/icons-material/ExpandMore";
import { NumberField } from "@/components/general-components/number-field.tsx";
import { fieldSx, topicSelectStyles } from "@/components/badges/badge-creator/stylings.ts";
import { TimeField } from "@mui/x-date-pickers/TimeField";
import type { ReactElement } from "react";
import { TopicSelector } from "@/components/general-components/topic-selector.tsx";
import { GetAllTopics } from "@/api/topic.ts";
import { components as selectComponents } from "react-select";

const NameMapping: Record<string, string> = {
    CorrectlyAnsweredItemsBadge: "Correctly answered items",
    TopicMasteredBadge: "Topic mastered",
    StreakProlongedBadge: "Streak prolonged",
    TestDoneAfterBadge: "Test done after a time",
    TestDoneBeforeBadge: "Test done before a time",
    BackToBackFailBadge: "Back to back failure",
};

const splitCamelCase = (name: string) =>
    name
        .replace(/Badge$/, "")
        .replace(/([a-z0-9])([A-Z])/g, "$1 $2")
        .replace(/([A-Z]+)([A-Z][a-z])/g, "$1 $2")
        .trim();

/**
 * Splits a PascalCase/camelCase identifier into spaced words and drops a trailing
 * "Badge". Used as a readable fallback for badge types missing a {@link NameMapping}
 * entry, e.g. "TestDoneAfterBadge" → "Test Done After".
 */

/**
 * Resolves a backend badge class name to a user-facing label, preferring the curated
 * {@link NameMapping} and falling back to {@link splitCamelCase}.
 */
export const FromNameMapping: (name: string) => string = (name: string) =>
    NameMapping[name] ?? splitCamelCase(name);

// Descriptive label and (i) helper text per parameter type (the EntryColumn from the backend)
export const ParameterMeta: Record<string, { label: string; description: string }> = {
    ItemAmount: {
        label: "Item amount",
        description: "The number of items a user must answer correctly to achieve this badge.",
    },
    FailureAmount: {
        label: "Failure amount",
        description:
            "The number of times a user has to fail the same topic in a row to achieve this badge.",
    },
    StreakCount: {
        label: "Streak count",
        description:
            "The number of consecutive days a user must keep a streak to achieve this badge.",
    },
    AfterTime: {
        label: "Time",
        description:
            "The time after which a user has to prolong their streak in order to achieve this badge",
    },
    BeforeTime: {
        label: "Time",
        description:
            "The time before which a user has to prolong their streak in order to achieve this badge",
    },
    Topic: {
        label: "Topic",
        description: "The topic a user must master to achieve this badge.",
    },
};

// Descriptive label and (i) helper text per overwritable property
export const OverwriteMeta: Record<string, { label: string; description: string }> = {
    name: {
        label: "Name",
        description: "Overwrites the default display name of the badge.",
    },
    description: {
        label: "Description",
        description: "Overwrites the default requirement description shown to users.",
    },
    category: {
        label: "Category",
        description: "Overwrites the default category used to group the badge.",
    },
    stamp: {
        label: "Stamp",
        description: "Overwrites the default image (stamp) shown for the badge.",
    },
};

// Renders the same chevron (ExpandMoreIcon) the MUI selects and accordion use, so every
// dropdown across the badge creator shares one indicator icon.
const TopicDropdownIndicator = (props: DropdownIndicatorProps<TopicLabel, false>) => (
    <selectComponents.DropdownIndicator {...props}>
        <ExpandMoreIcon fontSize="small" />
    </selectComponents.DropdownIndicator>
);

/** Props every parameter input in {@link InputRegistry} receives. */
interface ParameterInputProps {
    /** Current value of the parameter (type depends on the concrete input). */
    value: unknown;
    /** Updates the parameter's value in the form state. */
    setValue: (value: unknown) => void;
}

const NumberFieldWithProps = (p: ParameterInputProps) => (
    <NumberField
        value={p.value as number}
        setValue={p.setValue}
        sx={fieldSx}
        size="small"
        fullWidth
    />
);
const TimeFieldWithProps = (p: ParameterInputProps) => (
    <TimeField
        onChange={(value) => {
            if (value) p.setValue(value.toFormat("HH:mm:ss"));
        }}
        format="HH:mm"
        size="small"
        fullWidth
        sx={fieldSx}
    />
);

/**
 * Maps a backend parameter type (the `EntryColumn`) to the input component used to
 * edit it. Looked up by `ParameterDTO.type` when rendering a badge type's parameters.
 */
export const InputRegistry: Record<string, (p: ParameterInputProps) => ReactElement> = {
    Amount: NumberFieldWithProps,
    StreakCount: NumberFieldWithProps,
    TimeBorder: TimeFieldWithProps,
    TopicId: (p) => (
        <TopicSelector
            setSelectedTopic={(t) => p.setValue(t?.id)}
            getTopics={GetAllTopics}
            styles={topicSelectStyles}
            components={{ DropdownIndicator: TopicDropdownIndicator }}
        />
    ),
};
