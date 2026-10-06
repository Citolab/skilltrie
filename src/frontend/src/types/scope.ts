/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

/**
 * Scope Entity
 */
export interface Scope {
    id: number;
    name: string;
}

/**
 * TopicLabel Entity
 * This Object should ONLY ever be used as a key-value pair for the domain name and id.
 * The Topic entity might get extra properties in the future.
 */
export interface TopicLabel {
    scopeId: number;
    scopeName: string;
    ancestorId: number;
    ancestorName: string;
}

export type UserScopeInfo = TopicLabel & {
    proficiency?: number;
    mastered?: boolean;
    available: boolean;
};

export interface ScopeEdge {
    from: number;
    to: number;
    weight: number;
}
