/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { useState, useEffect } from "react";
import TemporaryLevelGenerator from "../../components/qti-player/temp-generate-level.tsx";
import type { TopicLabel } from "../../types/scope.ts";
import type { Setting } from "../../types/setting.ts";
import { GetRandomLevel } from "../../api/level.ts";
import { GetActiveSetting } from "../../api/settings.ts";
import { toast } from "react-toastify";
import { useLevelContext } from "../../contexts/level-context.tsx";
import { useNavigate } from "react-router-dom";

/**
 *
 * Pure wrapper for the QTI player.
 *
 */
export default function TopicSelection() {
    const { levelResponse, setLevelResponse } = useLevelContext();
    const [defaultLevelSize, setDefaultLevelSize] = useState<number>(10);
    const navigate = useNavigate();

    useEffect(() => {
        GetActiveSetting()
            .then((setting: Setting) => {
                setDefaultLevelSize(setting.levelSize);
            })
            .catch((error) => {
                toast.error("Could not load settings...");
                console.error(error);
            });
    }, []);

    useEffect(() => {
        if (levelResponse != null) navigate("/qti", { replace: true });
    }, [levelResponse]);

    function onLevelRequest(topic: TopicLabel | null) {
        if (topic == null) return;
        GetRandomLevel(topic.scopeId)
            .then(setLevelResponse)
            .catch((error) => {
                toast.error("Could not generate random level...");
                console.error(error);
            });
    }

    return (
        <>
            <TemporaryLevelGenerator onSubmit={onLevelRequest} levelSize={defaultLevelSize} />
        </>
    );
}
