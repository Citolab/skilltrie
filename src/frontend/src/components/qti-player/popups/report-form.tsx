/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { useState } from "react";
import { type ItemError, type ItemErrorStruct, type ReportPayload } from "../../../types/report";
import Select from "react-select";
import { CreateReport } from "../../../api/report";
import { usePostHog } from "posthog-js/react";
import { PopupLayout } from "../../general-components/popup-layout.tsx";

interface ReportFormProps {
    itemId: () => number;
    reportOptions: ItemErrorStruct[];
    reportDefault: ItemErrorStruct;
    onClose: () => void;
    open: boolean;
}

interface GroupedItemErrorStruct {
    label: ItemErrorStruct["category"]; // of type string
    options: ItemErrorStruct[];
}

function ReportForm(props: ReportFormProps) {
    const [reportType, setReportType] = useState<ItemError>(props.reportDefault.value);
    const [reportSuccess, setReportSuccess] = useState<boolean | null>(null);
    const posthog = usePostHog();

    /** group the different ItemErrorStructs together based on their category */
    const groupedItemErrors: GroupedItemErrorStruct[] = [
        ...new Set(props.reportOptions.map((r) => r.category)), // extract all categories, remove duplicates by constructing a set
    ].map((category) => ({
        // project into GroupedItemErrorStruct objects
        label: category,
        options: props.reportOptions.filter((r) => r.category === category),
    }));

    function handleReportSubmit(report: ReportPayload) {
        CreateReport(report)
            .then(() => {
                posthog.capture("question_reported", {
                    item_id: report.itemId,
                    error_type: report.itemError,
                });
                setReportSuccess(true);
                setTimeout(() => props.onClose(), 1000);
            })
            .catch(() => {
                setReportSuccess(false);
            });
    }

    return (
        <PopupLayout
            open={props.open}
            title={"Question report form"}
            headerClassName={"green"}
            onClose={props.onClose}
            primaryAction={{
                label: "Submit",
                onClick: () =>
                    handleReportSubmit({ itemId: props.itemId(), itemError: reportType }),
            }}
            secondaryAction={{ label: "Cancel" }}
        >
            <div>
                Please describe the issue with this question:
                <Select<ItemErrorStruct, false, GroupedItemErrorStruct>
                    className="items-center justify-between mt-2"
                    options={groupedItemErrors}
                    defaultValue={props.reportDefault}
                    onChange={(e) => {
                        if (e) setReportType(e.value);
                    }}
                />
                <p
                    className={`
                            text-center
                            my-6
                            bg-gray-300
                            p-1
                            rounded
                            font-bold
                            ${reportSuccess === null ? "hidden" : "inline"}
                            ${reportSuccess === false ? "text-red-500" : "text-black"}
                            ${reportSuccess === true ? "text-green-500" : "text-black"}
                        `}
                >
                    {reportSuccess
                        ? "Report submitted successfully!"
                        : "Failed to submit report, please try again."}
                </p>
            </div>
        </PopupLayout>
    );
}

export default ReportForm;
