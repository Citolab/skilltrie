/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import type { ABTest }  from "../../hooks/ab-testing.ts"; 

export const fancyTopicSuggestionAbTest: ABTest<boolean> = {
    flag: 'fancyTopicSuggestion',
    variants: {
        'control': false,
        'test': true,
    },
    default: false,
}