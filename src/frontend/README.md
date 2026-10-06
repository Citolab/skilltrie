# Front-End

### Required **NodeJS** Version:

Version 22.12 or later

### Required `npm` version

Anything later than version 7 should be fine

### **React** & **Vite** versions:

React: version 19.1.1
\
Vite: version 7
\
(for documentation purposes only; `npm` will take care of installation)

## Running:

1. `cd frontend` if you haven't done so already
2. `npm install` this might take a few seconds...

There are two ways to serve the front-end: one is through a dev-server, the other is to bundle/compile everything into static files and serve those through some other server:

### Running the dev-server:

Run `npm run dev`; **Vite** (the bundler) will automatically create a development server running on localhost. **Vite** is now serving the front-end. Any changes you make to files will be immediately reflected in the browser (this is what HMR, short for Hot Module Replacement, entails). This is probably what you'll be doing mostly during development.

#### Side Note: CORS

Because the front-end is now running on a dev server, your browser will prevent API calls from the front-end to the back-end server, because of CORS policy. By default, the browser will block cross origin resource sharing if the origins don't match, as is the case here: the dev server might run on, for example, localhost:5173 and the back-end server might run on, for example, localhost:5432.
\
There are at least two ways to work around this:

1. Explicitly set some specific CORS HTTP-headers on the back-end's preflight responses to tell the browser to allow the front-end to make requests to the back-end server.
2. Circumvent the browser by proxying all requests through Vite.

Currently, approach #2 is taken. To see the exact proxy settings, see `vite.config.ts`. This approach works because CORS is purely a browser-enforced security mechanism.

> _It may be advisable to lookup more on CORS if you don't know/remember what it is and in the future it would be good to ensure the project uses the best approach possible. There may be more viable options than the listed two._

### Building (for production)

Run `npm run build`; Now **Vite** will bundle all react files, stylesheets into raw `.html`, `.css` and `.js` files, and output these into a single output directory. The current output directory is configured to be `wwwroot` in the backend directory. It is now the responsibility of the back-end server to serve these static files (in this minimal example the back-end is already configured to do this).
\
With this approach CORS is no longer an issue, because the client is served from the same origin as the back-end.
