/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { useEffect, useState, useRef } from "react";
import { useLocation } from "react-router-dom";
import { type Item, itemTypes, languages, type SmallItemDto } from "../../../types/item";
import {
    GetItem,
    GetItems,
    SearchItemsByQuestionText,
    UpdateItem,
} from "../../../api/item";
import AdminTable from "../../../components/admin-dashboard/admin-table/admin-table";
import ObjectForm from "../../../components/admin-dashboard/admin-generic-object-form";
import AdminSearchAndPagination from "../../../components/admin-dashboard/admin-search-and-pagination";
import PopupWindow from "../../../components/admin-dashboard/admin-popup-window";
import GeneralButton from "../../../components/general-components/general-button.tsx";
import ChartDataLabels from "chartjs-plugin-datalabels"; // extension
import {
    BarElement,
    CategoryScale,
    Chart as ChartJS,
    Legend,
    LinearScale,
    Title,
    Tooltip,
} from "chart.js"; // dependency of react integration
import type { Scope, TopicLabel } from "../../../types/scope.ts"; // react integration
import { InitiateEvaluationChain, GetNextReportedItem, ItemEvaluation } from "../../../components/admin-dashboard/admin-item-dashboard/admin-item-evaluation.tsx";
import { ItemModal } from "../../../components/admin-dashboard/admin-item-dashboard/admin-item-statistics.tsx";
import Modal from "@mui/material/Modal";
import Fade from '@mui/material/Fade';
import Box from '@mui/material/Box';

// register components or the library is gonna say no
ChartJS.register(CategoryScale, LinearScale, BarElement, Title, Tooltip, Legend, ChartDataLabels);

/** helper function, removes truncated HTML tags from a given string */
function removeHTML(str: string): string {
    const container = document.createElement("div");

    container.innerHTML = str;
    const text = container.textContent ?? "";
    return text.replace(/<\/|</g, "");
}

export default function ItemDashboard() {
    const [items, setItems] = useState<SmallItemDto[]>([]);

    const [selectedItem, setSelectedItem] = useState<Item | null>(null);
    const [modalItem, setModalItem] = useState<Item | null>(null);

    const [formActive, setFormActive] = useState<boolean>(false);
    const [evaluateActive, setEvaluateActive] = useState<boolean>(false);

    const reportedItemQueue = useRef<string[]>([]);
    const reportedItemIndex = useRef<number>(0);

    const location = useLocation();
    const onGetNextReportedItem = () => GetNextReportedItem(reportedItemQueue, 
                                                            reportedItemIndex, 
                                                            setSelectedItem, 
                                                            setEvaluateActive)
                                                            
    const OnModalClose = () => {
        setEvaluateActive(false);
        reportedItemQueue.current = [];
        reportedItemIndex.current = 0;
    }

    const emptyReports = selectedItem ? selectedItem.reports.length === 0 : false;

    const objectKeys: (keyof SmallItemDto)[] = [
        "id",
        "active",
        "source",
        "type",
        "lang",
        "topics",
        "level",
        "responseType",
        "questionText",
    ];

    const headerNames = {
        responseType: "response type",
        questionText: "question text preview",
        lang: "language",
    };

    const [refreshItems, setRefreshItems] = useState<() => void>(() => {});

   useEffect(() => {
    if (!location.state?.initiateEvaluation) return;

    //useEffect runs twice for some reason, this (guard) is the solution that makes this work
    const guard = { cancelled: false };

    InitiateEvaluationChain(reportedItemQueue, onGetNextReportedItem, guard);

    return () => { guard.cancelled = true; };
    }, []);

    return (
        <div className="w-full bg-admin-surface p-4" data-testid="item-dashboard">
            <AdminSearchAndPagination<SmallItemDto>
                sortOptions={objectKeys}
                sortOptionNames={headerNames}
                searchPlaceholder="search question text"
                endpoints={{
                    pagination: (offset, range, sortColumn, sortOrder) => GetItems(offset, range, sortColumn, sortOrder),
                    search: (searchString) => SearchItemsByQuestionText(searchString),
                }}
                getObjects={(items) => setItems(items)}
                refresh={(refreshFunction) => setRefreshItems(() => refreshFunction)}
            />
            <div className="mb-2 ml-2 text-sm text-text-muted flex justify-between items-center">
                Tip: Click a row to view full question details. 
                <GeneralButton
                    type="button"
                    size="small"
                    onClick={() =>{ InitiateEvaluationChain(reportedItemQueue, onGetNextReportedItem) }}
                >
                    Review items
                </GeneralButton>               
            </div>
            
            <AdminTable
                rows={items}
                objectKeys={objectKeys}
                optionalKeys={["lang", "level", "responseType"]}
                headerNames={headerNames}
                transformColumnData={{
                    active: (cell) => ((cell as boolean) ? "active" : "inactive"),
                    topics: (cell) => (cell as Scope[])?.map((t) => t.name).join(", ") ?? "No topic found",
                    questionText: (cell) => removeHTML(cell as string),
                }}
                rowBackgroundColor={(item) => {
                    return item.active ? "bg-admin-table-LD" : "bg-admin-table-LD--inactive";
                }}
                tableActions={{
                    edit: {
                        action: (itemdto) => {
                            void GetItem(itemdto.id).then((item) => {
                                setSelectedItem(item);
                                setFormActive(true);
                            });
                        },
                    },
                    review: {
                        action: (itemdto) => {
                            void GetItem(itemdto.id).then((item) => {
                                setSelectedItem(item);
                                setEvaluateActive(true);
                                reportedItemQueue.current.push(item.id.toString());
                                onGetNextReportedItem();
                            })                           
                        },
                    },
                }}
                onRowClick={(item) => {
                    setModalItem(null);
                    void GetItem(item.id).then(setModalItem);
                }}
                renderExpandedRow={() => {
                    if (modalItem) {
                        return <ItemModal item={modalItem} />;
                    } else {
                        return null;
                    }
                }}
            />
            <PopupWindow
                isOpen= {formActive}
                onClose={() => setFormActive(false)}
                title="edit item"
                size="large"
            >
                <ObjectForm<Item>
                    errorMessage="[Error Message]"
                    initial={selectedItem ?? ({} as Item)}
                    onCancel={() => setFormActive(false)}
                    commitText="Edit"
                    onSubmit={(item) => {
                        void UpdateItem(item).then(() => refreshItems());
                        setFormActive(false);
                    }}
                    inputs={{
                        active: {
                            inputType: "boolean",
                        },
                        source: {
                            inputType: "immutable",
                        },
                        lang: {
                            inputType: "dropdown",
                            enumOptions: languages.slice(), // copy readonly array
                            label: "language",
                        },
                        responseType: {
                            label: "response type",
                            constraint: {
                                required: true,
                            },
                        },
                        type: {
                            inputType: "dropdown",
                            enumOptions: itemTypes.slice(), // copy readonly array
                        },
                        questionText: {
                            label: "question text",
                            inputType: "text",
                        },
                        answerExplanation: {
                            label: "answer explanation",
                            inputType: "text",
                        },
                        answers: {
                            childrenSectionName: "answers",
                            children: {
                                answerIdentifier: {
                                    inputType: "immutable",
                                    label: "answer identifier",
                                },
                                answerText: {
                                    inputType: "text",
                                    label: "answer text",
                                },
                                correct: {
                                    inputType: "boolean",
                                },
                            },
                        },
                    }}
                />
            </PopupWindow>
            
            <Modal 
                open={evaluateActive}
                onClose={OnModalClose}
                disableScrollLock={true}
            >
                <Fade in={evaluateActive}>
                    <Box sx={{
                            position: 'absolute',
                            top: '50%',
                            left: '50%',
                            transform: 'translate(-50%, -50%)',
                            width: '85vw',
                            height: '85vh',
                            maxWidth: emptyReports ? '30vw' : '90vw',
                            maxHeight: emptyReports ? '30vh' : '90vw',
                            bgcolor: 'background.paper',
                            borderRadius: 2,
                            boxShadow: 24,
                            p: 4,
                            display: 'flex',
                            flexDirection: 'column'
                        }}>
                            <button
                                type="button"
                                onClick={ () => OnModalClose() }
                                className="absolute top-0 right-0 w-8 h-8 cursor-pointer flex items-center justify-center 
                                            rounded-tr bg-red-200 hover:bg-red-300 text-black-500"
                            > X </button>

                            <ItemEvaluation
                                selectedItem={selectedItem}
                                reportedItemQueue={reportedItemQueue}
                                reportedItemIndex={reportedItemIndex}
                                GetNextReportedItem={onGetNextReportedItem}
                                refreshItems={refreshItems}
                            />
                    </Box>
                </Fade>
            </Modal>
        </div>
    );
}
