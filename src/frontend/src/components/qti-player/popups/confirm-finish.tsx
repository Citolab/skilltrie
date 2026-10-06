/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { PopupLayout } from "../../general-components/popup-layout.tsx";

interface ConfirmFinishProps {
    onFinish: () => void;
    onContinue: () => void;
    open: boolean;
}

export const ConfirmFinish = (props: ConfirmFinishProps) => {
    return (
        <PopupLayout
            open={props.open}
            title={"Level not complete!"}
            headerClassName={"cyan"}
            onClose={props.onContinue}
            primaryAction={{ label: "Finish Level", onClick: props.onFinish }}
            secondaryAction={{ label: "Continue Level"}}
        >
            There are still questions in this level unanswered. Are you sure you want to finish this level already?
        </PopupLayout>
    );
};
