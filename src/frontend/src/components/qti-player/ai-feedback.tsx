/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { useState, useEffect } from "react";
import { FetchAiFeedback } from "../../api/ai";

interface AiFeedbackProps {
    testId: number;
}

export default function AiFeedbackPage({ testId }: AiFeedbackProps) {
    const [feedback, setFeedback] = useState<string>("");
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);

    const getFeedback = async () => {
        setLoading(true);
        setError(null);
        try {
            await FetchAiFeedback(testId, (token) => { 
                setFeedback(prev => prev + token);
            });
        } catch (err) {
            console.error(err);
            setError("Failed to load feedback.");
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        void getFeedback();
    }, [testId]);

    if (loading) return <div className="p-4 bg-white text-black">Loading AI feedback...</div>;

    if (error) return <div className="p-4 bg-white text-black">{error}</div>;

    return (
        <div className="p-4 h-[80vh] overflow-y-auto bg-white text-black border border-black rounded">
            <h1 className="font-bold mb-4 text-lg">AI Feedback for Test {testId}</h1>

            <p className="whitespace-pre-wrap">{feedback}</p>

            <button
                className="px-4 py-2 mt-4 bg-white text-black border border-black rounded hover:bg-gray-100"
                onClick={getFeedback}
                disabled={loading}
            >
                Refresh Feedback
            </button>
        </div>
    );
}
