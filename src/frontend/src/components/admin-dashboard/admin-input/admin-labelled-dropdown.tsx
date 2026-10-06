/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

interface AdminLabelledDropdownProps<T extends object> {
    label: string;
    options: (keyof T)[];
    optionNames?: Partial<Record<keyof T, string>>;
    defaultOption: keyof T;
    onChange: (value: keyof T) => void;
}

function AdminLabelledDropdown<T extends object>(props: AdminLabelledDropdownProps<T>) {
    const options: string[] = props.options.map((option) =>
        props.optionNames?.[option] ? props.optionNames[option] : (option as string)
    );

    return (
        <div className="flex w-max rounded focus-within:ring-2 focus-within:ring-borderActive">
            <div className="select-none cursor-default px-4 py-2 text-sm rounded-l border border-admin-border--inactive bg-primary  text-admin-button-text-LD flex items-center">
                {props.label}
            </div>

            <div className="flex items-center border outline-none rounded-r bg-admin-input border-admin-border--inactive">
                <select
                    className="px-4 py-2 text-sm w-auto outline-none bg-inherit text-admin-text-LD cursor-pointer capitalize"
                    defaultValue={props.defaultOption as string}
                    onChange={(e) => {
                        const value = e.target.value;

                        const index = options.indexOf(value);

                        return props.onChange(props.options[index]);
                    }}
                >
                    {options.map((option) => (
                        <option
                            key={String(option)}
                            value={String(option)}
                            className="bg-admin-input text-admin-text-LD cursor-pointer"
                        >
                            {String(option)}
                        </option>
                    ))}
                </select>
            </div>
        </div>
    );
}

export default AdminLabelledDropdown;
