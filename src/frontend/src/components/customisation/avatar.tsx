/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

interface avatarProps {
    RiveComponent: React.FC<React.CanvasHTMLAttributes<HTMLCanvasElement>>;
    previewText: string;
}

export default function Avatar({ RiveComponent, previewText }: avatarProps) {
    return (
        <div className="flex flex-col h-full w-full relative">
            <div className="relative flex w-full h-full ">
                <RiveComponent className="w-full h-full bg-cyan-500" />
            </div>
            <div className="absolute m-auto left-0 flex justify-center p-4 text-4xl font-bold text-white">
                {previewText}
            </div>
        </div>
    );
}
