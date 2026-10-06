/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { type ComputedContext } from "@citolab/qti-components";
import ComputedContextHelper from "../../../utils/qti/computed-context-helper";
import { QtiButton, QtiGradientButton } from "./qti-buttons";
import type { QtiPlayerNavigation } from "../main-page/qti-player.tsx";

interface QtiTestButtonsProps {
    computedContext: ComputedContext;
    playerLoading: boolean;
    onNavigation: (index: QtiPlayerNavigation) => void;
    onTestEnd?: () => void;
}

function QtiTestButtons(props: QtiTestButtonsProps) {
    const contextHelper = new ComputedContextHelper(props.computedContext);

    return (
        <>
            {!contextHelper.isLastItem &&
                (contextHelper.activeItemCompleted ? (
                    <QtiButton
                        variant="primary"
                        onClick={() => {
                            props.onNavigation(contextHelper.activeIndex);
                        }}
                        disabled={props.playerLoading}
                        className="min-w-45"
                    >
                        Next question
                    </QtiButton>
                ) : (
                    <QtiButton
                        variant="yellow"
                        onClick={() => props.onNavigation(contextHelper.activeIndex)}
                        disabled={props.playerLoading}
                        className="min-w-45"
                    >
                        Skip question
                    </QtiButton>
                ))}
            {contextHelper.isLastItem && (
                <QtiGradientButton onClick={props.onTestEnd}>Finish Level</QtiGradientButton>
            )}
        </>
    );
}

export default QtiTestButtons;
