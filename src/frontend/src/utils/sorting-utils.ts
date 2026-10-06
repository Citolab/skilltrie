/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

export default class StringSorter {
    /** Levenshtein similarity scorer */
    private static levenshteinDistance(a: string, b: string): number {
        const dp: number[][] = Array.from({ length: a.length + 1 }, () =>
            Array<number>(b.length + 1).fill(0)
        );

        for (let i = 0; i <= a.length; i++) dp[i][0] = i;
        for (let j = 0; j <= b.length; j++) dp[0][j] = j;

        for (let i = 1; i <= a.length; i++) {
            for (let j = 1; j <= b.length; j++) {
                const cost = a[i - 1] === b[j - 1] ? 0 : 1;
                dp[i][j] = Math.min(dp[i - 1][j] + 1, dp[i][j - 1] + 1, dp[i - 1][j - 1] + cost);
            }
        }

        return dp[a.length][b.length];
    }

    /** sort list of strings based on similarity to reference string,
     * using the Levenshtein distance as a similarity metric.
     * the original array is left unmutated. Polynomial runtime.
     * @param reference - the reference string
     * @param arr - the to-be-sorted array
     * @returns a new sorted array
     */
    static sortByLevenshtein(reference: string, arr: string[]): string[] {
        return [...arr].sort(
            (x, y) =>
                this.levenshteinDistance(reference, x) - this.levenshteinDistance(reference, y)
        );
    }

    /** compare strings based on substring */
    private static substringSimilarity(a: string, b: string): number {
        a = a.toLowerCase();
        b = b.toLowerCase();

        let score = 0;
        for (let len = 2; len <= 3; len++) {
            for (let i = 0; i <= a.length - len; i++) {
                const sub = a.slice(i, i + len);
                if (b.includes(sub)) score += len;
            }
        }

        // normalize length
        const avgLen = (a.length + b.length) / 2;
        return score / avgLen;
    }

    /** sort list of strings based on similarity to reference string using substring similarity.
     * the original array is left unmutated. Better performance than Levenshtein.
     * @param reference - the reference string
     * @param arr - the to-be-sorted array
     * @returns a new sorted array
     */
    static sortBySubstring(reference: string, arr: string[]): string[] {
        return [...arr].sort(
            (x, y) =>
                this.substringSimilarity(reference, y) - this.substringSimilarity(reference, x)
        );
    }
}
