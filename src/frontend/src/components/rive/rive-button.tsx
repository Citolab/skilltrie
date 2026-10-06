/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { EventType, useRive } from "@rive-app/react-canvas";

function RiveButton() {
    const { rive, RiveComponent } = useRive({
        src: window.location.origin + "/rive/button.riv",
        autoplay: false,
        animations: ["klik", "hover", "idle 1"],
    });

    rive?.on(EventType.Loop, () => {
        rive?.stop();
    });

    return (
        <RiveComponent
            onClick={() => rive && rive.play("klik")}
            onMouseEnter={() => {
                rive?.stop();
                rive?.play("hover");
            }}
            onPointerOut={() => {
                rive?.stop();
                if (!rive?.isPlaying) rive?.play("idle 1");
            }}
        />
    );
}

export default RiveButton;
