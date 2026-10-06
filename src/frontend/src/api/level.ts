/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { FetchJson } from "../utils/fetch-json.ts";
const controllerUrl = "/api/level/";
import type {
    LevelAnswerRequestDto,
    UserAnswer,
    UserAnswerRequestDto,
    LevelResponse,
} from "../types/level.ts";
import type { ComputedContext } from "@citolab/qti-components";

/**
 * Builds a query with user ID, level size, and topic labels, then requests a random level from the backend API.
 * */
export function GetRandomLevel(topicId: number, userId: number = 1) {
    const params = new URLSearchParams({
        userId: String(userId),
    });

    return FetchJson<LevelResponse>(controllerUrl.concat(`randomlevel?${params.toString()}`), {
        method: "POST",
        body: JSON.stringify(topicId),
        headers: { "Content-Type": "application/json" },
    });
}

/**
 * Sends a single user’s answer to the backend API as JSON and returns the saved answer response.
 * */
export async function SubmitUserAnswer(dto: UserAnswerRequestDto): Promise<UserAnswer> {
    return await FetchJson<UserAnswer>(controllerUrl.concat("submitAnswer"), {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(dto),
    });
}

/**
 * Logs and submits a full set of level answers to the backend API, returning an array of user answer responses.
 * */
export async function SubmitUserLevel(
    levelResponse: LevelResponse,
    computedContext: ComputedContext
): Promise<UserAnswer[]> {
    // prettier-ignore
    const answers : UserAnswerRequestDto[] =
        computedContext
            .testParts[0]   // -> our assessments always consist of a single test part,
            .sections[0]    // -> and a single section within that test part,
            .items          // -> containing all our items for this particular test.
            .filter(item => item.completionStatus !== "not_attempted")
            .map(item => {
                return {
                    itemId: levelResponse.itemIds[item.index! - 1],
                    levelId: levelResponse.levelId,
                    correct: item.correct ?? item.score === item.maxScore,
                    completionStatus: "completed",
                    answer:
                        item.variables.find(v => v.identifier.includes("qti"))?.value as string // open / numerical
                        ??
                        null,
                    answerIdentifier:
                        // techinically string[] is also allowed, but so far we're not using
                        // cardinality: multiple for our QTI questions (= more than 1 answer allowed)
                        item.variables.find(v => v.identifier === "RESPONSE")?.value as string  // multiple choice
                }
            });

    const levelAnswer: LevelAnswerRequestDto = {
        levelResultId: levelResponse.levelId,
        answers: answers,
    };

    return await FetchJson<UserAnswer[]>(controllerUrl.concat("submitLevel"), {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(levelAnswer),
    });
}
