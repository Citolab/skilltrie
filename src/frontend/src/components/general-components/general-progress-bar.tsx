/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

export function ProgressBar({
    progress,
    progressNeeded,
}: {
    progress: number;
    progressNeeded: number;
}) {
    const pct = Math.min(Math.round((progress / progressNeeded) * 100), 100);
    const isComplete = progress >= progressNeeded;

    return (
        <div className="flex-col gap-y-6 h-full">
            <div className="flex justify-between">
                <span
                    className={`text-sm font-medium ${isComplete ? "text-yellow-500" : "text-gray-500"}`}
                >
                    {isComplete ? "✓ Complete" : "Progress"}
                </span>
                <span className="text-sm text-gray-400">
                    {progress} / {progressNeeded} ({pct}%)
                </span>
            </div>
            <div className="h-4 rounded-full bg-gray-100 border border-gray-200 overflow-hidden">
                <div
                    className={`h-full rounded-full transition-all duration-300 ${isComplete ? "bg-yellow-500" : "bg-gray-500"}`}
                    style={{ width: `${pct}%` }}
                />
            </div>
        </div>
    );
}
