/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { useEffect, useState } from "react";
import { useSearchParams } from "react-router-dom";
import AdminButton from "./admin-input/admin-button";
import AdminLabelledDropdown from "./admin-input/admin-labelled-dropdown";
import AdminSearchBar from "./admin-input/admin-search-bar";
import AdminToggleButton from "./admin-input/admin-toggle-button";

interface EndPointsConfig<T> {
    /** pagination fetcher */
    pagination: (offset: number, range: number, sortColumn: string, sortOrder: string) => Promise<T[]>;
    /** search fetcher */
    search?: (searchString: string) => Promise<T[]>;
}

interface AdminSearchAndPaginationProps<T> {
    /** on which properties of the generic object we can sort */
    sortOptions: (keyof T)[];
    /** allows for renaming the sort options */
    sortOptionNames?: Partial<Record<keyof T, string>>;
    /** placeholder for the searchbar */
    searchPlaceholder?: string;

    endpoints: EndPointsConfig<T>;
    /** the objects that were fetched through one of the endpoints */
    getObjects: (objects: T[]) => void;
    /** refetch the objects */
    refresh: (refreshFunction: () => void) => void;
}

function AdminSearchAndPagination<T extends object>(props: AdminSearchAndPaginationProps<T>) {
    const [searchParams, setSearchParams] = useSearchParams();

    const [range, setRange] = useState<number>(Number(searchParams.get("range")) || 15);
    const [offset, setOffset] = useState<number>(Number(searchParams.get("offset")) || 0);
    const [endOfPagination, setEndOfPagination] = useState<boolean>(false);

    const [objects, setObjects] = useState<T[]>([]);

    const [lastFetchMethod, setLastFetchMethod] = useState<"pagination" | "search" | null>(
        "pagination"
    );
    const [lastSearchString, setLastSearchString] = useState<string>("");

    const [sortColumn, setSortColumn] = useState<keyof T>(props.sortOptions[0]);
    const [sortOrder, setSortOrder] = useState<"ascending" | "descending">("ascending");

    // fetch via pagination
    function fetchPagination() {
        void props.endpoints.pagination(offset, range, String(sortColumn), sortOrder).then((fetched) => {
            setObjects(fetched);
            setEndOfPagination(fetched.length < range);
            setLastFetchMethod("pagination");
        });
    }

    // fetch via search
    function fetchSearch(searchString: string) {
        if (searchString.length < 3) return;

        void props.endpoints?.search?.(searchString).then((fetched) => {
            setObjects(fetched);
            setEndOfPagination(fetched.length < range);
            setLastFetchMethod("search");
        });
    }

    // refresh function
    function refreshObjects() {
        if (lastFetchMethod === "pagination") {
            fetchPagination();
        } else if (lastFetchMethod === "search") {
            fetchSearch(lastSearchString);
        }
    }

    // communicate refresh function to parent component
    useEffect(() => {
        props.refresh(() => {
            refreshObjects();
        });
    }, [sortOrder, lastFetchMethod, range, offset]); // prevent reading old state

    useEffect(() => {
        fetchPagination();
    }, [sortColumn, sortOrder]);

    // pagination effect
    useEffect(() => {
        setSearchParams({ offset, range } as any);
        fetchPagination();
    }, [offset, range]);

    // communicate to parent of objects update
    useEffect(() => {
        props.getObjects(objects);
    }, [objects]);

    return (
        <div className="mt-2 mb-2 flex xl:items-center xl:gap-2 gap-4 justify-between flex-col-reverse xl:flex-row">
            <div className="flex sm:flex-row flex-col sm:items-center gap-2">
                {props.endpoints?.search !== undefined && ( // only show if search endpoint is defined
                    <AdminSearchBar
                        label="search:"
                        onChange={(searchString) => {
                            setLastSearchString(searchString);
                            fetchSearch(searchString);
                        }}
                        placeholder={props.searchPlaceholder ?? ""}
                    />
                )}
                <AdminLabelledDropdown
                    label="Sort on"
                    options={props.sortOptions}
                    optionNames={props.sortOptionNames}
                    defaultOption={props.sortOptions[0]}
                    onChange={(key) => setSortColumn(key)}
                />
                <AdminToggleButton
                    states={["ascending", "descending"]}
                    selectedState={sortOrder}
                    stateSelected={(state) => setSortOrder(state as any)}
                    embed={(state) => `Sort by: ${state}`}
                />
            </div>

            <div className="flex items-center gap-2">
                <AdminLabelledDropdown
                    label="Range"
                    options={["10", "15", "20", "50"]}
                    defaultOption={String(range)}
                    onChange={(key) => setRange(Number(key))}
                />
                <AdminButton
                    onClick={() => setOffset(Math.max(0, offset - range))}
                    disabled={offset === 0}
                >
                    Previous
                </AdminButton>
                <AdminButton onClick={() => setOffset(offset + range)} disabled={endOfPagination}>
                    Next
                </AdminButton>
            </div>
        </div>
    );
}

export default AdminSearchAndPagination;
