/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { FetchJson } from "../utils/fetch-json.ts";
import type { ScopeEdge, TopicLabel, UserScopeInfo } from "../types/scope.ts";
const controllerUrl = "/api/topics/";

export async function GetAllTopics() {
    return FetchJson<TopicLabel[]>(controllerUrl);
}

export async function GetUserTopics() {
    return FetchJson<TopicLabel[]>(controllerUrl.concat(`usertopics`));
}

export async function GetUserTopicInfo() {
    return FetchJson<UserScopeInfo[]>(controllerUrl.concat("user-status"));
}

export async function GetTopicDependencies() {
    return FetchJson<ScopeEdge[]>(controllerUrl.concat("dependencies"));
}

export async function GetNextSuggestedTopic() {
    return FetchJson<TopicLabel | null>(controllerUrl.concat("next-topic"));
}
