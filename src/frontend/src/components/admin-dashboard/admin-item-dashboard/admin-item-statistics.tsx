/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { easeOut, motion } from "motion/react";
import { type Item } from "../../../types/item";
import AdminCard from "../admin-misc/admin-card";
import {
    type ChartData,
    type ChartOptions,
} from "chart.js"; // dependency of react integration
import { Bar } from "react-chartjs-2";

export function ItemModal({ item, darkTheme = true }: { item: Item, darkTheme?: boolean }) {
    // combined chosen count of all answers of this item,
    // note: technically different from appearanceCount!!
    const totalAnswers = item.answers.reduce((acc, v) => {
        return acc + v.chosen;
    }, 0);

    // chart data
    const data: ChartData<"bar"> = {
        labels: Array.from(item.answers.keys(), (i) => `Answer ${i + 1}`), // labels under each bar
        datasets: [
            {
                label: "Correct answer(s) count",
                data: item.answers.map((a) => (a.correct ? a.chosen : 0)),
                backgroundColor: "rgb(112, 234, 112)",
                borderColor: "rgb(150, 205, 150)",
                borderWidth: 1,
                barPercentage: 0.9,
                categoryPercentage: 1,
            },
            {
                label: "Incorrect answer(s) count",
                data: item.answers.map((a) => (!a.correct ? a.chosen : 0)),
                backgroundColor: "rgb(250, 100, 102)",
                borderColor: "rgb(235, 150, 150)",
                borderWidth: 1,
                barPercentage: 0.9,
                categoryPercentage: 1,
            },
        ],
    };

    // chart options
    const options: ChartOptions<"bar"> = {
        responsive: true,
        plugins: {
            legend: {
                position: "bottom",
                align: "center",
                labels: {
                    color: darkTheme ? "#eee" : "#000",
                },
            },
            datalabels: {
                color: darkTheme ? "#eee" : "#000",
                anchor: "end",
                align: "right",
                formatter: (value) => {
                    // show percentage labels ontop of bars
                    return `${Math.round((value / totalAnswers) * 100)}%`;
                },
                display: (context) => {
                    // don't display labels ontop of bars that won't show up anyway
                    return context.dataset.data[context.dataIndex] !== 0;
                },
                font: {
                    weight: "bold",
                },
            },
        },
        indexAxis: "y", // make this bar chart horizontal
        scales: {
            x: {
                max: item.appearanceCount,
                ticks: {
                    color: darkTheme ? "#ccc" : "#000", // color of the labels of this axis
                },
                // grid lines (for this axis)
                grid: {
                    display: true,
                    color: darkTheme ? "rgba(255,255,255,0.15" : "rgba(0,0,0,0.15",
                },
            },
            y: {
                // technically we 'stack' the correct answers ontop of the incorrect answers for
                // each answer column, but because each column either has 0 incorrect answers, and
                // x correct answers (or vice versa), only one bar per column is displayed
                // this allows us to get a legend entry for both incorrect and correct answers
                // (see the two datasets above)
                stacked: true,
                ticks: {
                    color: darkTheme ? "#ccc" : "#000", // color of the labels of this axis
                },
                // grid lines (for this axis)
                grid: {
                    display: false,
                },
            },
        },
    };

    return (
        <>
            <motion.div
                className="w-full flex flex-col md:flex-row md:items-start md:justify-between gap-4 p-4"
                initial={{ height: 0 }}
                animate={{ height: "auto" }}
                transition={{ type: "tween", duration: 0.25, ease: easeOut }}
                exit={{ height: 0 }}
                style={{ overflow: "hidden" }}
            >
                <div className="flex-1 text-lg font-normal">
                    <h1 className="text-2xl font-medium mb-3"
                        style={{ color: darkTheme ? "#ccc" : "#000" }}
                    >
                        Item ID: {item.id}
                    </h1>
                    <span className="mb-4 block"
                          style={{ color: darkTheme ? "#ccc" : "#000" }}
                    >
                        Item appeared {item.appearanceCount} times
                    </span>

                    <div
                        style={{
                            height: `calc(var(--spacing) * ${item.answers.length * 20})`,
                        }}
                    >
                        {item.answers.length > 1 && <Bar data={data} options={options} />}
                    </div>
                </div>

                <div className="w-full md:w-1/2 2xl:w-1/3 flex flex-col gap-2">
                    <AdminCard title="Question text" defaultOpen={true}>
                        <div
                            className="text-sm font-[monospace] tracking-tighter mt-4"
                            dangerouslySetInnerHTML={{
                                __html: item.questionText.replaceAll(
                                    /<qti-text-entry-interaction.*?>/g,
                                    ""
                                ),
                            }}
                        />
                    </AdminCard>
                    <AdminCard title={item.answers.length === 1 ? "Answer" : "Answers"}>
                        <div className={"text-sm font-[monospace] tracking-tighter mt-4"}>
                            {item.answers.map((answer, index) => {
                                return (
                                    <div className="flex flex-row gap-2" key={index}>
                                        <div>{index + 1}:</div>
                                        <div
                                            className={`${answer.correct ? "text-emerald-600" : "text-rose-600"} text-sm font-[monospace] tracking-tighter font-semibold`}
                                            dangerouslySetInnerHTML={{ __html: answer.answerText }}
                                        />
                                    </div>
                                );
                            })}
                        </div>
                    </AdminCard>
                    {item.answerExplanation && (
                        <AdminCard title="Answer explanation">
                            <div
                                className="text-sm font-[monospace] tracking-tighter mt-4"
                                dangerouslySetInnerHTML={{ __html: item.answerExplanation }}
                            />
                        </AdminCard>
                    )}
                </div>
            </motion.div>
        </>
    );
}