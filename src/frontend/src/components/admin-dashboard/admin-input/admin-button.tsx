/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

interface AdminButtonProps {
    children: React.ReactNode;
    disabled?: boolean;
    onClick?: (e: unknown) => void;
    type?: "submit" | "button";
}

function AdminButton(props: AdminButtonProps) {
    return (
        <button
            type={props.type}
            className="px-4 py-2 text-sm rounded cursor-pointer borderDefault bg-primary hover:bg-primary-dark text-text-light disabled:opacity-75 disabled:bg-primary-tint  disabled:text-text-dark disabled:cursor-not-allowed"
            disabled={props.disabled ?? false}
            onClick={(e) => props.onClick?.(e)}
        >
            {props.children}
        </button>
    );
}

export default AdminButton;
