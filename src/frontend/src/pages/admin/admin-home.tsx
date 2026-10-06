/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */
import "../../index.css";
import type { MetricDTO } from "../../types/metrics";
import { useState, useEffect } from "react";
import type { ReactNode } from "react";
import { Grid, Card, CardContent, Button, Typography, Box } from "@mui/material";
import type { SxProps, Theme } from "@mui/material";
import { LineChart } from "@mui/x-charts";
import { GetDashboardMetrics } from "../../api/researcher";
import { useNavigate } from "react-router-dom";

// Page

export default function AdminHome() {
    const [metrics, setMetrics] = useState<MetricDTO | null>(null);
    const navigate = useNavigate();

    useEffect(() => {
        GetDashboardMetrics()
            .then((json) => setMetrics(json))
            .catch((err) => {
                console.error("Failed to fetch dashboard statistics:", err);
            });
    }, []);

    const chartDates =
        metrics?.testsTakenPastWeek.map((d) =>
            new Date(d.date).toLocaleDateString("en-GB", { month: "short", day: "numeric" })
        ) ?? [];
    const chartCounts = metrics?.testsTakenPastWeek.map((d) => d.count) ?? [];

    return (
        <div className="p-18">
            <main className="max-w-7xl mx-auto w-full">
                <Typography
                    variant="h3"
                    component="h1"
                    align="center"
                    sx={{
                        color: "var(--color-text-primary)",
                        fontFamily: "var(--font-title)",
                        fontWeight: "Bold",
                        mb: 6,
                    }}
                >
                    Researcher Dashboard
                </Typography>

                <Grid container spacing={3}>
                    {/* Left column: single metrics */}
                    <Grid size={{ xs: 12, md: 4 }}>
                        <Box
                            sx={{
                                display: "flex",
                                flexDirection: "column",
                                gap: 3,
                                height: "100%",
                            }}
                        >
                            <Box sx={{ display: "flex", flexDirection: "row", gap: 3, flex: 1 }}>
                                <DashboardTile
                                    title="Total Users"
                                    value={metrics?.studentCount.toString() ?? "..."}
                                    sx={{ flex: 1 }}
                                />
                                <DashboardTile
                                    title="Active Users"
                                    value={metrics?.activeUserCount.toString() ?? "..."}
                                    sx={{ flex: 1 }}
                                />
                            </Box>

                            <DashboardTile
                                title="Reported Items"
                                value={metrics?.flaggedItemCount.toString() ?? "..."}
                                sx={{ flex: 1 }}
                            >
                                <Button
                                    variant="outlined"
                                    size="small"
                                    onClick={() => navigate('../questions', { state: {initiateEvaluation: true } }) }
                                >
                                    Review items
                                </Button>
                            </DashboardTile>
                        </Box>
                    </Grid>

                    {/* Right column: chart metrics */}
                    <Grid size={{ xs: 12, md: 8 }}>
                        <DashboardTile
                            title="Tests Taken (Past 7 Days)"
                            sx={{ backgroundColor: "var(--color-nav-LD)" }}
                        >
                            <LineChart
                                xAxis={[
                                    {
                                        data: chartDates,
                                        scaleType: "point",
                                    },
                                ]}
                                yAxis={[
                                    {
                                        tickMinStep: 1,
                                        max:
                                            chartCounts.length > 0
                                                ? Math.max(...chartCounts) * 1.15
                                                : 5,

                                        tickInterval: (value) => {
                                            const maxVal =
                                                chartCounts.length > 0
                                                    ? Math.max(...chartCounts)
                                                    : 0;

                                            if (maxVal <= 5) {
                                                return true;
                                            }

                                            const step = maxVal <= 20 ? 5 : maxVal <= 100 ? 20 : 50;
                                            return value % step === 0;
                                        },

                                        valueFormatter: (value: number) =>
                                            Math.round(value).toString(),
                                    },
                                ]}
                                series={[{ data: chartCounts, color: "red" }]}
                                height={300}
                                margin={{ right: 40 }}
                                sx={{
                                    "& .MuiChartsAxis-line": {
                                        stroke: "var(--color-metric-value) !important",
                                    },
                                    "& .MuiChartsAxis-tick": {
                                        stroke: "var(--color-metric-value) !important",
                                    },
                                    "& .MuiChartsAxis-tickLabel": {
                                        fill: "var(--color-metric-value) !important",
                                    },
                                }}
                            />
                        </DashboardTile>
                    </Grid>
                </Grid>
            </main>
        </div>
    );
}

// Dashboard tile component

interface DashboardTileProps {
    title: string;
    value?: string;
    children?: ReactNode;
    sx?: SxProps<Theme>;
}

const DashboardTile = ({ title, value, children, sx = {} }: DashboardTileProps) => (
    <Card sx={{ height: "100%", borderRadius: 2, backgroundColor: "var(--color-nav-LD)", ...sx }}>
        <CardContent sx={{ display: "flex", flexDirection: "column", height: "100%" }}>
            <Box
                sx={{
                    display: "flex",
                    justifyContent: "space-between",
                    alignItems: "center",
                    mb: 1,
                }}
            >
                <Typography variant="overline" sx={{ color: "var(--color-primary)", fontSize: 16 }}>
                    {title}
                </Typography>
            </Box>
            {value && (
                <Box sx={{ flex: 1, display: "flex", alignItems: "center" }}>
                    <Typography
                        variant="h4"
                        sx={{ color: "var(--color-metric-value)", fontSize: 42 }}
                    >
                        {value}
                    </Typography>
                </Box>
            )}
            {children}
        </CardContent>
    </Card>
);
