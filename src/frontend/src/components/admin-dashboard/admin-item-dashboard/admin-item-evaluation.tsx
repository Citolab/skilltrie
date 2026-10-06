/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import React, { useEffect, useState } from "react";
import { motion, AnimatePresence } from "motion/react";
import { SkipForward } from 'lucide-react';
import { toast } from "react-toastify";
import { type Item} from "../../../types/item.ts";
import { ItemModal } from "./admin-item-statistics.tsx";
import {
    ActivateItem,
    DeactivateItem,
    GetItem,
    ResolveReports,
} from "../../../api/item.ts";
import { GetAllReports } from "../../../api/report.ts";
import { getErrorLabel } from "../../../types/report.ts"

export const InitiateEvaluationChain = (
    reportedItemQueue: React.RefObject<string[]>,
    GetNextReportedItem: () => void,
    guard: { cancelled: boolean } = { cancelled: false }
): void => {
    GetAllReports().then((reports) => {
        if (guard.cancelled) return;
        const sortedIds = Object.entries(
            reports.reduce((acc, report) => {
                acc[report.itemId] = (acc[report.itemId] || 0) + 1;
                return acc;
            }, {} as Record<string, number>)
        )
        .sort(([, a], [, b]) => b - a)
        .map(([id]) => id);

        reportedItemQueue.current = [...sortedIds];
        GetNextReportedItem();
    });
    
};
    
//squashes and sorts all items into a list with format -> List<[Id, reportsSum]>
export const GetNextReportedItem = (
    reportedItemQueue: React.RefObject<string[]>,
    reportedItemIndex: React.RefObject<number>,
    setSelectedItem: (item: any) => void,
    setEvaluateActive: (value: boolean) => void
): void => {
    if (reportedItemIndex.current < reportedItemQueue.current.length)
    {
        GetItem(Number(reportedItemQueue.current[reportedItemIndex.current]))
            .then((currentReportedItem) => setSelectedItem(currentReportedItem));
        
        reportedItemIndex.current++;
        setEvaluateActive(true);
    }
    else //if there are no more items in the queue -> reload to default values and stop chain
    {  
        reportedItemQueue.current = [];
        reportedItemIndex.current = 0;  
        
        toast.success("Item(s) reviewed succesfully!");
        setEvaluateActive(false);
    }       
}

//squashes and sorts all reports of selected item into a list with format -> List<[ReportId, sum]>
const ReduceItemReports = (
    selectedItem: Item | null
) : Array<[string, number]> => {
    const sortedIds = Object.entries(
        selectedItem?.reports?.reduce((acc, report) => {
            acc[report.itemError] = (acc[report.itemError] || 0) + 1
            return acc
        }, {} as Record<string, number>) ?? {})
        .sort(([, a], [, b]) => b - a);

    return sortedIds
}

interface ItemEvaluationProps {
    selectedItem: Item | null;
    reportedItemQueue: React.RefObject<any[]>;
    reportedItemIndex: React.RefObject<number>;
    GetNextReportedItem : () => void;
    refreshItems: () => void;
}

export function ItemEvaluation({
    selectedItem,
    reportedItemQueue,
    reportedItemIndex,
    GetNextReportedItem,
    refreshItems,
}: ItemEvaluationProps) {
    //used to collapse the graph when reviewing multiple items after eachother
    const [statisticsActive, setStatisticsActive] = useState<boolean>(false);

    useEffect(() => {
        setStatisticsActive(false);
    }, [selectedItem]);

    const emptyReports = selectedItem ? selectedItem.reports.length === 0 : false;

    return (
        <AnimatePresence mode="wait">
            <motion.div
                key={selectedItem?.id}
                initial={{ opacity: 0, x: 20 }}
                animate={{ opacity: 1, x: 0 }}
                exit={{ opacity: 0, x: -20 }}
                transition={{ duration: 0.3, ease: 'easeInOut' }}
                className="flex flex-col flex-1 min-h-0 h-full"
                contentEditable={false}
            >
                {!selectedItem || emptyReports ? (
                    <div className="flex flex-row min-h-screen justify-center text-gray-500">No reports found.</div>
                ) : (
                    <div className="flex h-full">
                        <div className="flex flex-col flex-1 pr-2 border-r h-full">
                            <div className="flex border-b justify-between items-end">
                                <h2 className="text-2xl font-semibold mb-2">
                                    Item evaluation form
                                </h2>
                                <div className="flex relative justify-between gap-2 pr-2">
                                    <button 
                                        className={`pl-2 pr-2 ${statisticsActive ? "h-6" : "h-8 bg-primary text-white"} -mb-px mt-auto cursor-pointer 
                                                    rounded-tl rounded-tr border border-b-0`}
                                        onClick={() => setStatisticsActive(false)}
                                    >
                                        Qti-player
                                    </button>
                                    <button 
                                        className={`pl-2 pr-2 ${statisticsActive ? "h-8 bg-primary text-white" : "h-6"} -mb-px mt-auto cursor-pointer 
                                                    rounded-tl rounded-tr border border-b-0`}
                                        onClick={() => setStatisticsActive(true)}
                                    >
                                        Item statistics
                                    </button>
                                </div>
                            </div>
                            <div className="bg-white flex-1 p-4 overflow-y-auto min-h-0" 
                                style={{ display: statisticsActive ? 'none' : '' }}
                            >
                                <qti-item>             
                                    <item-container item-url={'/api/qti/get-item/' + selectedItem.id}></item-container>
                                </qti-item>
                                <div className="h-[10%]"></div>
                                <div className="border-2 border-green-300">
                                    <h2 className="text-l font-semibold"><span className="font-medium">Correct answer:</span> {selectedItem.answers
                                            .filter((answer) => answer.correct)
                                            .map((answer) => answer.answerText.replace(/<[^>]*>/g, ''))
                                            .join(', ')}
                                    </h2>
                                </div>
                            </div>
                        
                            <div className="bg-blue-100 flex-1 overflow-y-auto min-h-0"
                                style={{ display: statisticsActive ? '' : 'none' }}
                            >
                                <ItemModal item={selectedItem} darkTheme={false} />
                            </div>
                        </div>

                        <div className="border-l p-1 bg-gray-100 flex flex-col w-[35%] min-h-0 overflow-y-auto h-full">
                            <div className="border-b">
                                <h2 className="text-2xl font-semibold mb-2">
                                    Reports: {selectedItem.reports.length}
                                </h2>
                            </div>
                            <ul className="gap-2 flex-1 overflow-y-auto min-h-0">
                                {ReduceItemReports(selectedItem).map(([report, amount], index) => (
                                    <li key={index} className="p-3 bg-gray-100 rounded-lg border-1">
                                        {getErrorLabel(report)} <span className="text-m font-bold"> x{amount} </span>
                                    </li>
                                ))}
                            </ul>

                            {reportedItemQueue.current.length != 1 && (
                                <div className="flex flex-shrink-0 border-t-2">
                                    <h2 className="text-2xl font-semibold mb-2">
                                        {reportedItemQueue.current.length == (reportedItemIndex.current)
                                            ? "Last item to evaluate"
                                            : `Items to go: ${reportedItemQueue.current.length - reportedItemIndex.current}`}
                                    </h2>
                                </div>
                            )}

                            <div className="flex flex-shrink-0 gap-2 justify-center mt-4">
                                <button
                                    type="button"
                                    onClick={() => {void ActivateItem(selectedItem.id)
                                                            .then(() => refreshItems())
                                                            .then(() => ResolveReports(selectedItem.id))
                                                            .then(() => GetNextReportedItem())
                                                            }}
                                    className="flex-1 px-4 rounded-lg bg-blue-500 hover:bg-blue-600 text-white cursor-pointer"
                                >
                                    {selectedItem.active ? "Resolve" : "Enable & resolve"}
                                </button>
                                <button
                                    type="button"
                                    onClick={() => {void ActivateItem(selectedItem.id)
                                                            .then(() => refreshItems())
                                                            .then(() => GetNextReportedItem())
                                                            }}
                                    disabled={selectedItem.active}
                                    className={`flex-1 px-4 py-5 rounded-lg cursor-pointer disabled:cursor-not-allowed 
                                                ${selectedItem.active ? "bg-gray-300" : "bg-blue-300 hover:bg-blue-400"}`}
                                >
                                    {selectedItem.active ? "Enabled" : "Enable"}
                                </button>
                                <button
                                    type="button"
                                    onClick={() => {void DeactivateItem(selectedItem.id)
                                                            .then(() => refreshItems())
                                                            .then(() => GetNextReportedItem())
                                                            }}
                                    disabled={!selectedItem.active}
                                    className={`flex-1 px-4 py-4 rounded-lg cursor-pointer disabled:cursor-not-allowed 
                                                ${!selectedItem.active ? "bg-gray-300" : "bg-red-300 hover:bg-red-400"}`}
                                >
                                    {!selectedItem.active ? "Disabled" : "Disable" }
                                </button>
                                <button
                                    type="button"
                                    onClick={ () => void GetNextReportedItem() }
                                    className="flex pl-2 pr-2 text-m items-center justify-center py-4 rounded-lg cursor-pointer bg-amber-400 hover:bg-amber-500"
                                >
                                    <SkipForward />
                                </button>
                            </div>
                        </div>
                    </div>
                )}
        </motion.div>
    </AnimatePresence>
    )
}
