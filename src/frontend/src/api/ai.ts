/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { FetchJson } from "../utils/fetch-json.ts";

const controllerUrl = "api/ai/";

export function QuestionAI() {
    return FetchJson<string>(controllerUrl.concat("createSampleQuestion"));
}

export async function FetchAiFeedback(
    testId: number,
    onChunk: (chunk: string) => void
): Promise<void> {
    const response = await fetch(controllerUrl.concat(`feedback/${testId}`));
    const reader = response.body!.getReader();
    const decoder = new TextDecoder();

    while (true) {
        const { done, value } = await reader.read();
        if (done) break;
        onChunk(decoder.decode(value));
    }
}
