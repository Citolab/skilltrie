/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { Layout, Fit, Alignment, useRive } from "@rive-app/react-canvas";

// Hardcoded for now
const riveFile = "../rive/avatar.riv";
const artboard = "AvatarBackground";
const stateMachineName = "State Machine 1";

export function useAvatar() {
    const { rive, RiveComponent } = useRive({
        src: riveFile,
        autoplay: true,
        artboard: artboard,
        stateMachines: stateMachineName,
        layout: new Layout({
            fit: Fit.Cover,
            alignment: Alignment.Center,
        }),
    });

    function setRiveInput(name: string, value: number) {
        if (!rive) return;
        const input = rive.stateMachineInputs(stateMachineName)?.find((i) => i.name === name);
        if (input) input.value = value;
    }

    return { riveComponent: RiveComponent, setRiveInput };
}
