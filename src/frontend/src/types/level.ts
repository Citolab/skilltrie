/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

export interface LevelAnswerRequestDto {
    levelResultId: number;
    answers: UserAnswerRequestDto[];
}

export interface UserAnswer {
    id: number;
    itemId: number;
    levelId: number;
    answer?: string | null;
    correct: boolean;
    completionStatus: string;
}

export interface UserAnswerRequestDto {
    itemId: number;
    levelId: number;
    answer: string | null;
    answerIdentifier: string | null;
    correct: boolean;
    completionStatus: string;
}

export interface LevelResponse {
    assessmentXml: string;
    itemIds: number[];
    levelId: number;
}
