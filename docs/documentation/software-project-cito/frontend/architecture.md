
# Frontend 'Architecture' / Layout

<style scoped>
li {list-style-type: none;}
</style>

The frontend has the following folder structure (listing the most important parts):

### `src/`
> `api/`
\
> `components/`
\
> `pages/`
\
> `router/`
\
> `types/`
\
> `utils/`
\
> `contexts/`

Below follows an explanation for each directory, assuming you're familiar with React on a conceptual level.

## Pages and Components

In React, pretty much any function that returns a `JSX.Element` is considered a component.
Nevertheless, we treat components that encapsulate an entire 'page' as special and group
them under the `pages/` folder. Pages are just components that group multiple components together.

## Router

Since the frontend is a single page application (SPA), we need to 'fake' the routing to different pages on the client-side, in the browser.
Modern browsers expose a [Navigation API](https://developer.mozilla.org/en-US/docs/Web/API/Navigation_API) which one
can use to implement this. However, as with most frameworks it is not necessary to reinvent the wheel.
We use the [React Router](https://reactrouter.com/) to implement this for us.
\
\
In `router/index.tsx` lives the config for the React Router, specifying which URLs should route to which components (pages):

```tsx
export const router = createBrowserRouter([
    {
        path: "/",
        element: <RootRedirect />,
    },
    {
        path: "/landing",
        element: <LaunchPage />,
    },
    {
        path: "/login",
        element: <LoginPage />,
    },
    ...
]);
```

## API

Instead of using the [fetch API](https://developer.mozilla.org/en-US/docs/Web/API/Fetch_API) directly in React components, we abstract this logic away into separate functions.
Each file in `api/` usually corresponds to a single controller class on the backend:
#### `api/`
> `item.ts`
\
> `qti.ts`
\
> `report.ts`
\
> ...

Within each such file, each function almost always maps cleanly to its complementary controller method on the backend:

::: details
The `FetchJson` function below is a wrapper for the `fetch` API.
:::

```ts
const controllerUrl = "/api/settings/";

export function GetSettings(offset: number, range: number) : Promise<Setting[]> {
    const params = new URLSearchParams({
        offset: String(offset),
        range: String(range),
    });

    return FetchJson<Setting[]>(controllerUrl + `get-settings?${params.toString()}`);
}

export function GetActiveSetting() : Promise<Setting> {
    return FetchJson<Setting>(controllerUrl + `active-setting`);
}
```

## Types

Because the front- and backend are not in the same language (and we're using TypeScript instead of JavaScript), we have to duplicate the type definitions of each of the backend's DTOs (Data Transfer Objects) / Models;
that's the only purpose of the files in this folder.
\
Example:

::: code-group

```ts:line-numbers [currency.ts]
export interface Currency {
    id: number;
    name: string;
    sprite: string;
}

export interface UserCurrency {
    userId: number;
    currencyId: number;
    amount: number;
}

export type UserCurrencyUpdateDto = Omit<UserCurrency, "userId">;
```

:::


## Contexts

Contains contexts that are available throughout the whole application if they are children of the belonging context

## Utils

Contains a grouping of various utility functions, so these don't appear intermittently in components or other parts of the frontend -> prevents clutter & duplicate code.