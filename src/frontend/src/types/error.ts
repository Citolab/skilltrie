/* 
 * This program has been developed by students from the bachelor Computer Science at 
 * Utrecht University within the Software Project course.
 * © Copyright Utrecht University (Department of Information and Computing Sciences)
 * Licensed under the MIT License. See the LICENSE file in the project root for details.
 */


/** 
 * body of the error inside the `error` string of {@link ServerError}
*/
export interface ApiError {
    type: string;
    title: string;
    status: number;
    detail: string;
    traceId: string;
}

/** Body of an `error` returned by the server */
export interface ServerError {
    error: string;
    response: Response;
}