/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import type { Scope } from "./scope.ts";
import type { Report } from "./report.ts"

export const itemTypes = [
    "MultipleChoice",
    "Open",
    "Numerical",
] as const satisfies readonly string[];
export const itemSources = ["LLM", "Imported", "Databank"] as const satisfies readonly string[];
export const languages = ["English", "Dutch"] as const satisfies readonly string[];

export type ItemType = (typeof itemTypes)[number];
export type ItemSource = (typeof itemSources)[number];
export type Language = (typeof languages)[number];

/**
 * Answer Entity
 */
export interface Answer {
    id: number;
    itemId: number; // foreign key
    answerIdentifier?: string | null;
    answerText: string;
    chosen: number;
    correct: boolean;
}

/**
 * Item Entity
 */
export interface Item {
    id: number;
    active: boolean;
    type: ItemType;
    source: ItemSource;
    responseType?: string | null; // e.g. "conceptual", "interpreting graph", ...
    lang?: string | null;
    level?: string | null; // difficulty
    answerExplanation?: string | null;
    questionText: string;
    appearanceCount: number;
    topics: Scope[];
    answers: Answer[];
    reports: Report[];
}

/**
 * SmallItemDto (omits answers, and correct answer explanation)
 */
export type SmallItemDto = Omit<Item, "answerExplanation" | "answers">;
