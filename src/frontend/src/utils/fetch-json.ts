/*
 * This program has been developed by students from the bachelor Computer Science at
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */

//generic util for making HTTP requests and safely parsing JSON responses.
export async function FetchJson<T>(url: string, options?: RequestInit): Promise<T> {
    // window.location.origin -> ensure we query from base-url, not relative to the current page we're on
    const response = await fetch(
        window.location.origin + import.meta.env.BASE_URL.replace(/\/$/, "") + url,
        options
    );
    const contentType = response.headers.get("content-type");
    const isJson = contentType?.includes("application/json");

    let body: unknown;

    if (isJson) {
        try {
            body = await response.json();
        } catch {
            body = null;
        }
    } else {
        body = await response.text();
    }

    if (!response.ok) {
        const errorMessage = `Request failed with status ${response.status}: ${
            typeof body === "string" ? body : JSON.stringify(body)
        }`;

        console.error(errorMessage);

        // eslint-disable-next-line @typescript-eslint/naming-convention
        return new Promise((_, reject) =>
            // eslint-disable-next-line @typescript-eslint/prefer-promise-reject-errors
            reject({ error: errorMessage, response: response })
        );
    }

    // Handle empty or non-JSON responses
    if (response.status === 204 || body === null || body === "") {
        return undefined as T;
    }

    return body as T;
}
