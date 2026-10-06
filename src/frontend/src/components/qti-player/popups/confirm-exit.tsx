/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { PopupLayout } from "../../general-components/popup-layout.tsx";

interface ConfirmExitProps {
    onQuit: () => void;
    onStay: () => void;
    open: boolean;
}

export const ConfirmExit = (props: ConfirmExitProps) => {    
    return (
        <PopupLayout
            open={props.open}
            title={"Are you sure you want to quit the level?"}
            headerClassName={"red"}
            onClose={props.onStay}
            primaryAction={{ label: "Quit", onClick: props.onQuit}}
            secondaryAction={{ label: "Stay"}}
        >
            If you decide to quit, you will lose all your progress made in this level!
        </PopupLayout>
    );
};
