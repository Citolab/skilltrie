/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { useEffect, useState } from "react";

import AdminTableHeader from "./admin-table-header";
import React from "react";

interface AdminTable<T extends object> {
    /** table data, one generic object per row */
    rows: T[];
    /** show only properties of object with the following keys */
    objectKeys: (keyof T)[];
    /** columns that may be sacrificed on smaller screens */
    optionalKeys?: (keyof T)[];
    /** allows renaming the headers to a more readable format */
    headerNames?: Partial<Record<keyof T, string>>;

    /** formatter functions for specific 'columns' (properties) */
    transformColumnData?: Partial<Record<keyof T, (cell: unknown) => string>>;

    /** record of buttons to attach to each row, with callback function onClick,
     * and callback function for changing the button label.
     */
    tableActions?: Record<string, { action: (row: T) => void; labelChange?: (row: T) => string }>;

    /** optional function to determine the background color of a row based on row data */
    rowBackgroundColor?: (row: T) => string;

    /** optional action to take when a row is clicked, includes row data */
    onRowClick?: (row: T, rowIndex: number) => void;

    /** callback function to render optional modal that shows when a row is clicked */
    renderExpandedRow?: (row: T, rowIndex: number) => React.ReactNode | null;
}

function AdminTable<T extends object>(props: AdminTable<T>) {
    const [expandedRowIndex, setExpandedRowIndex] = useState<number | null>(null);

    //setFinalKeys(props.objectKeys.filter(column => !props.optionalKeys.includes(column)));

    const tableData: string[][] = props.rows.map((object) => {
        return props.objectKeys.map((key) => {
            if (props.transformColumnData?.[key]) {
                // use specified stringification function for this property
                return props.transformColumnData[key](object[key]);
            } else {
                // no stringification function specified? -> just stringify
                return String(object[key]);
            }
        });
    });

    const tableActions: Record<string, (rowIndex: number) => void> = {};

    for (const key in props.tableActions) {
        tableActions[key] = (rowIndex) => {
            props.tableActions![key].action?.(props.rows[rowIndex]);
        };
    }

    const tableHeaders: string[] = props.objectKeys
        .map((key) => props.headerNames?.[key] ?? String(key))
        .concat(new Array(Object.keys(tableActions).length).fill("")); // one-liner to account for the buttons in the header

    const optionalHeaders: string[] = props.objectKeys
        .map((key, index) => (props.optionalKeys?.includes(key) ? index : -1))
        .filter((i) => i !== -1)
        .map((i) => tableHeaders[i]);

    useEffect(() => setExpandedRowIndex(null), [props.rows]);

    const container = document.getElementById("scroll-container");

    // scroll table horizontally if it overflows on small screens
    container?.addEventListener("wheel", (e) => {
        if (container.scrollWidth > container.clientWidth) {
            // prevent vertical scroll
            e.preventDefault();

            container.scrollLeft += e.deltaY;
        }
    });

    return (
        <div id="scroll-container" className="overflow-x-auto">
            {" "}
            {/* make table scrollable for phones & other small devices */}
            <table className="table-auto w-full text-sm text-left rtl:text-right ">
                <AdminTableHeader headers={tableHeaders} optionalHeaders={optionalHeaders} />
                {/* TODO: perhaps abstract away <tr> & <td> styling */}
                <tbody>
                    {tableData.map((row, rowIndex) => {
                        const isExpanded = expandedRowIndex === rowIndex;

                        const modal = props.renderExpandedRow?.(props.rows[rowIndex], rowIndex);

                        const modalEmpty = !modal;

                        return (
                            <>
                                <tr
                                    className={`border-2 border-admin-table-border hover:bg-admin-table-LD--hover cursor-pointer ${props.rowBackgroundColor?.(props.rows[rowIndex]) ?? "bg-admin-table-LD"} ${isExpanded ? "bg-admin-table-LD--hover" : ""}`}
                                    key={rowIndex}
                                    onClick={() => {
                                        props.onRowClick?.(props.rows[rowIndex], rowIndex);
                                        setExpandedRowIndex((prev) =>
                                            prev === rowIndex ? null : rowIndex
                                        );
                                    }}
                                >
                                    {row.map((cell, cellIndex) => {
                                        return (
                                            <td
                                                className={`${props.optionalKeys?.includes(props.objectKeys[cellIndex]) ? "hidden min-[1750px]:table-cell" : ""} first:px-6 px-3 py-4 truncate max-w-xs text-admin-text-LD`}
                                                key={cellIndex}
                                            >
                                                {cell}
                                            </td>
                                        );
                                    })}
                                    {Object.keys(props.tableActions ?? {}).map(
                                        (name, cellIndex) => {
                                            return (
                                                <td className="px-3 py-4 max-w-xs" key={cellIndex}>
                                                    <button
                                                        onClick={(e) => {
                                                            e.stopPropagation();
                                                            props.tableActions![name].action?.(
                                                                props.rows[rowIndex]
                                                            );
                                                        }}
                                                        className="truncate capitalize underline cursor-pointer font-medium  text-admin-table-text-LD hover:underline"
                                                    >
                                                        {props.tableActions![name].labelChange?.(
                                                            props.rows[rowIndex]
                                                        ) ?? name}
                                                    </button>
                                                </td>
                                            );
                                        }
                                    )}
                                </tr>
                                {isExpanded && props.renderExpandedRow && (
                                    <tr
                                        className={`
                                                    ${modalEmpty ? "hidden bg-pink-400" : ""}
                                                    bg-gray-950 border-gray-700 border-t-2 border-b-2
                                                    `}
                                    >
                                        <td
                                            colSpan={
                                                tableData[0].length +
                                                Object.keys(props.tableActions ?? {}).length
                                            }
                                            className="max-w-xs"
                                        >
                                            {props.renderExpandedRow(
                                                props.rows[rowIndex],
                                                rowIndex
                                            )}
                                        </td>
                                    </tr>
                                )}
                            </>
                        );
                    })}
                </tbody>
            </table>
        </div>
    );
}

export default AdminTable;
