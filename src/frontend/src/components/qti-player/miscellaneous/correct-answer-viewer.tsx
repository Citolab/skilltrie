/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import type ComputedContextHelper from "../../../utils/qti/computed-context-helper.ts";
import { FetchJson } from "../../../utils/fetch-json.ts";
import { useState } from "react";
import Switch from "@mui/material/Switch";
import FormControlLabel from "@mui/material/FormControlLabel";

export const CorrectAnswerViewer = ({
    contextHelper,
    visible = true,
}: {
    contextHelper: ComputedContextHelper;
    visible?: boolean;
}) => {
    const [answer, setAnswer] = useState("");
    const [showCorrectAnswer, setShowCorrectAnswer] = useState(true);
    let href;
    try {
        href = "/" + contextHelper.activeItem.href?.concat("/answer");
    } catch (e) {
        href = "";
    }

    if (!href) return;

    FetchJson<string>(href).then((a) => setAnswer(a));

    const ShowAnswerToggle = () => {
        return (
            <FormControlLabel
                control={
                    <Switch
                        checked={showCorrectAnswer}
                        onChange={(_, checked) => setShowCorrectAnswer(checked)}
                        defaultChecked
                    />
                }
                label="Juiste antwoord tonen"
            />
        );
    };

    if (!visible) return;
    return (
        <div>
            <p className={showCorrectAnswer ? "visible" : "invisible"}>
                Juist antwoord:
                <span className="block text-green-600">
                    {answer && answer.replace("<p>", "").replace("</p>", "")}
                </span>
            </p>
            <ShowAnswerToggle />
        </div>
    );
};
