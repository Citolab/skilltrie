/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

import { describe, it, vi, expect, beforeEach } from "vitest";
import { CheckAuth } from "../../src/api/auth";

describe("checkAuth", () => {
    beforeEach(() => {
        vi.restoreAllMocks();
    });

    it("returns authorized true when response is 200 with email", async () => {
        vi.stubGlobal(
            "fetch",
            vi.fn().mockResolvedValue({
                status: 200,
                json: () => ({ email: "test@example.com" }),
            })
        );

        const result = await CheckAuth();
        expect(result).toEqual({
            authorized: true,
            user: { email: "test@example.com" },
        });
    });

    it("returns authorized false when response is not 200", async () => {
        vi.stubGlobal(
            "fetch",
            vi.fn().mockResolvedValue({
                status: 401,
                json: () => ({}),
            })
        );

        const result = await CheckAuth();
        expect(result).toEqual({
            authorized: false,
            user: { email: "" },
        });
    });

    it("retries on fetch failure and succeeds on second attempt", async () => {
        const fetchMock = vi
            .fn()
            .mockRejectedValueOnce(new Error("Network error"))
            .mockResolvedValueOnce({
                status: 200,
                json: () => ({ email: "retry@example.com" }),
            });

        vi.stubGlobal("fetch", fetchMock);

        const result = await CheckAuth("/api/auth/pingauth", 1, 0);
        expect(result).toEqual({
            authorized: true,
            user: { email: "retry@example.com" },
        });
        expect(fetchMock).toHaveBeenCalledTimes(2);
    });

    it("throws error after exceeding maxRetries", async () => {
        const fetchMock = vi.fn().mockRejectedValue(new Error("Fail"));

        vi.stubGlobal("fetch", fetchMock);

        await expect(CheckAuth("/api/auth/pingauth", 2, 0)).rejects.toThrow("Fail");
        expect(fetchMock).toHaveBeenCalledTimes(3); // initial + 2 retries
    });
});
