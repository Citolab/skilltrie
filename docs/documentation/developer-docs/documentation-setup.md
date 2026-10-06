# Documentation Setup

This file discusses how the documentation was set up.
\
All of the documentation and config lives under `/docs` in the project root.

## Vitepress

Documentation is written in Markdown (`.md`) files. To generate a documentation website from these markdown files, we use [Vitepress](https://vitepress.dev/). Vitepress is a JavaScript based
**Static Site Generator**. It takes in `.md` files and transpiles these to static `.html` pages (+ other static assets). Static in this context means no server-side logic is needed to render the pages
of the website.

### How to generate

- `npm run docs:dev` starts a dev-server that reflects any changes you make to the documentation live (editing config files might require restarting the dev-server).
- `npm run build` actually transpiles all the documentation to a static site.

### Auto Docs

We also generate automatic documentation for the backend code. More documentation on that can be found [here](/developer-docs/auto-docs).

## Vitepress config

Vitepress config lives in `/docs/.vitepress/config.mts`. Apart from the standard configuration we also use an additional extension, the [VitePress Sidebar](https://vitepress-sidebar.cdget.com/guide/getting-started) package;
it allows for a more advanced sidebar setup (the navbar on the left).

## Hosting

Hosting a static site is quite simple. We used a simple [Nginx](https://nginx.org/) server to serve our static files.
\
Nevertheless, some caveats to watch out for:
- If you've disabled the `.html` extensions from showing up in the URLs / links of your pages in the Vitepress config, 
make sure that your HTTP webserver is configured to deal with this. (e.g. `try_files $uri $uri.html` in Nginx)
- The index file (file that's supposed to map to `/`) in Vitepress is called `index.html`. You may have to set this explicitly depending on your webserver.
- You might want to hide your documentation site behind a basic authentication layer if you don't want your documentation to be public or scrapeable by search engines.

## Documentation

### VitePress

- [Getting started](https://vitepress.dev/guide/getting-started)
- [Config + Misc. reference](https://vitepress.dev/guide/getting-started)


### VitePress Sidebar

- [General docs](https://vitepress-sidebar.cdget.com/guide/getting-started)
- [Options/Config reference](https://vitepress-sidebar.cdget.com/guide/options)