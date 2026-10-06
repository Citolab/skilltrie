/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

const metroPositions: Record<string, { x: number; y: number }> = {
    // Measurement level
    "measurement level":      { x: -300,  y: -200 },

    // Descriptive statistics
    "mean":                   { x: -100,  y: 150 },
    "mode":                   { x: -300,  y: 250 },
    "median":                 { x: -500,  y: 250 },
    "range":                  { x: -700,  y: 50  },
    "standard deviation":     { x: -200,  y: 500 },
    "interquartile range":    { x: -600,  y: 500 },
    "outliers":               { x: -300,  y: 1000},
    "variance":               { x: -100,  y: 750 },
    "covariance":             { x: 0,     y: 1000},
    "correlation":            { x: -100,  y: 1250},

    // Distributions
    "normal":                 { x: -300,  y: 1500},
    "central limit theorem":  { x: -400,  y: 1750},
    "t-distribution":         { x: -400,  y: 2000},

    // Inferential statistics
    "sampling distributions": { x: -300,  y: 2250},
    "t-statistic":            { x: -400,  y: 2500},
    "hypothesis":             { x: -300,  y: 2750},
    "statistical errors":     { x: -150,  y: 3000},
    "power":                  { x: -50,   y: 3250},
    "significance level":     { x: -450,  y: 3000},
    "p-value":                { x: -600,  y: 3250},
};

export const layoutMetroMobile = (nodes: any[], edges: any[]) => ({
    nodes: nodes.map(node => {
        if (node.type === "domain") {
            return { ...node, targetPosition: "top", sourcePosition: "bottom" };
        }
        const position = metroPositions[node.data.objectInfo?.name] ?? { x: 0, y: 0 };
        return {
            ...node,
            targetPosition: "top",
            sourcePosition: "bottom",
            position,
            data: {
                ...node.data,
                positionX: position.x,
                positionY: position.y,
            }
        };
    }),
    edges,
});
