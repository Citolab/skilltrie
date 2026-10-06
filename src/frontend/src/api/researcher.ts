/* * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import type { MetricDTO } from "../types/metrics.ts";
import { FetchJson } from "../utils/fetch-json.ts";

const controllerUrl = "/api/researcher/dashboard/";

/** Get researcher dashboard metrics
 *
 * @returns `Promise<MetricDTO>`
 */
export function GetDashboardMetrics() {
    return FetchJson<MetricDTO>(controllerUrl.concat("metrics"));
}
