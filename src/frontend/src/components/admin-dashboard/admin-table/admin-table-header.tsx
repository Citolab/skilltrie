/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

interface AdminTableHeaderProps {
    headers: string[];
    optionalHeaders?: string[];
}

function AdminTableHeader(props: AdminTableHeaderProps) {
    return (
        <thead>
            <tr className="bg-black text-white text-xs uppercase">
                {props.headers.map((header, index) => (
                    <th
                        key={index}
                        scope="col"
                        className={`${props.optionalHeaders?.includes(header) ? "hidden min-[1750px]:table-cell" : ""} first:px-6 px-3 py-3 select-none hover:underline truncate`}
                    >
                        {header}
                    </th>
                ))}
            </tr>
        </thead>
    );
}

export default AdminTableHeader;
