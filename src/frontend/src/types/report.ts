/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

export interface ItemErrorStruct {
    readonly value: ItemError;
    readonly label: string;
    readonly category: "question" | "answer" | "graphics" | "other"; // Will be used later for selection component filtering (eg items has no image)
}

export type ItemError =
    | "QTextEmpty"
    | "QTextIncorrect"
    | "ATextEmpty"
    | "ATextIncorrect"
    | "AnswersWrong"
    | "GraphicsUnavailable"
    | "GraphicsFaulty"
    | "GraphicsUnrelated"
    | "OtherError";

export interface Report {
    id: number;
    itemId: number;
    itemError: ItemError;
}
export type ReportPayload = Omit<Report, "id">;

export const itemErrors: readonly ItemErrorStruct[] = [
    { value: "QTextEmpty" , label: "Question text is empty", category: "question" },
    { value: "QTextIncorrect", label: "Question text is incorrect", category: "question" },
    { value: "ATextEmpty", label: "Answer text is empty", category: "answer" },
    { value: "ATextIncorrect", label: "Answer text is incorrect", category: "answer" },
    { value: "AnswersWrong", label: "Answers are wrong", category: "answer" },
    { value: "GraphicsUnavailable", label: "Image is missing", category: "graphics" },
    { value: "GraphicsFaulty", label: "Image quality is low", category: "graphics" },
    {
        value: "GraphicsUnrelated",
        label: "Image does not relate to question",
        category: "graphics",
    },
    { value: "OtherError", label: "Something else is wrong", category: "other" },
];

export const getErrorLabel = (value: string) => 
    itemErrors.find(e => e.value === value)?.label ?? value;
