/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import AdminButton from "./admin-button";

interface AdminToggleButtonProps {
    /** selected state -> it's the parent's responsibility to manage state */
    selectedState: string;
    /** called if a new state has been selected */
    stateSelected: (state: string) => void;
    /** all possible states for this button */
    states: string[];
    /** embed the state string in some other string */
    embed?: (s: string) => string;
}

function AdminToggleButton(props: AdminToggleButtonProps) {
    return (
        <AdminButton
            onClick={() =>
                props.stateSelected(
                    props.states[
                        (props.states.indexOf(props.selectedState) + 1) % props.states.length
                    ]
                )
            }
        >
            {props.embed?.(props.selectedState) ?? props.selectedState}
        </AdminButton>
    );
}

export default AdminToggleButton;
