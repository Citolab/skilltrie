/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { useFeatureFlagVariantKey } from "posthog-js/react";

export type ABTest<T> = {
    // The feature flag key on posthog
    flag: string;
    // A map from variant to component, making it so we dont need if-else statements
    variants: Record<string, T>;
    // The fall-back component if we have an illegal flag / no flag
    default: T;
}

// Gets the feature flag from Posthog and returns the component
export function useABTest<T>(test: ABTest<T>): T {
    const variant = useFeatureFlagVariantKey(test.flag)
    return test.variants[String(variant) ?? ''] ?? test.default
}