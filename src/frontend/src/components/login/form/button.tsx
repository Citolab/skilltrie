/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

interface ButtonProps {
    label: string;
    onClick?: () => void;
    type?: "button" | "submit";
    backgroundImage: string;
}

export default function Button({ label, onClick, type = "button", backgroundImage }: ButtonProps) {
    return (
        <button
            type={type}
            onClick={onClick}
            className="w-[50%] aspect-1444/583 bg-cover bg-center bg-no-repeat text-white text-lg font-medium cursor-pointer"
            style={{ backgroundImage: `url(${backgroundImage})` }}
        >
            {label}
        </button>
    );
}
