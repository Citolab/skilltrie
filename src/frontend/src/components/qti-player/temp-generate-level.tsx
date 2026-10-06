/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { useState } from "react";
import { GetUserTopics } from "../../api/topic";
import type { TopicLabel } from "../../types/scope.ts";
import { useNavigate } from "react-router-dom";
import { TopicSelector } from "@/components/general-components/topic-selector.tsx";

interface TemporaryLevelGeneratorProps {
    onSubmit: (topic: TopicLabel | null) => void;
    levelSize: number;
}

function TemporaryLevelGenerator(props: TemporaryLevelGeneratorProps) {
    const [selectedTopic, setSelectedTopic] = useState<TopicLabel | null>(null);
    const navigate = useNavigate();

    return (
        <div className="flex flex-col items-start p-4 gap-2">
            <h2 className="block mb-2 font-semibold text-gray-300">
                This is the pre-level screen. Leave the input field below empty to get random topics
                or choose one or more topics. If your selection of topics contains an insufficient
                amount of questions, the Start Level button will not work.
                <br></br>
                The options in this list are based on your current proficiencies per topic.
                <br></br>
                <br></br>
                Selected Topics:
            </h2>
            <TopicSelector
                getTopics={GetUserTopics}
                setSelectedTopic={setSelectedTopic}
                className="min-w-150"
            />
            <div className="flex flex-row gap-x-4">
                <button
                    onClick={() => props.onSubmit(selectedTopic)}
                    className="bg-primary text-white px-4 py-2 rounded hover:bg-primary-dark mt-4 cursor-pointer"
                >
                    Start Level
                </button>
                <button
                    onClick={() => navigate("/home")}
                    className="bg-red-700 text-white px-4 py-2 rounded hover:bg-red-600 mt-4 cursor-pointer"
                >
                    Back to Home
                </button>
            </div>
        </div>
    );
}

export default TemporaryLevelGenerator;
