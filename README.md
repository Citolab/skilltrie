# **Virtueel Leersysteem** Software Project

## Group D - _Stastiftics_ & _SkillTrie_

## Introduction
This ReadMe provides a small overview of how to set up the project. More detailed information about the application itself and how to set up other things can be found in the `docs` folder.

## Installation

Please ensure you meet the prerequisites!

## Prerequisites

- [.NET 8.0 Core](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) runtime
- [NodeJS v22.x](https://nodejs.org/en/download/archive/v22.22.0) (or later)
- NPM, though you may also try your luck with other package managers
- [docker-desktop](https://www.docker.com/products/docker-desktop)

Optional:

- [PostgreSQL](https://www.postgresql.org/) (only tested with version 16.11 or later)

## How to run the project

For development, one typically runs the frontend separately from the backend on a dev-server. Though this is not strictly necessary for running the project (more on this below).

### Database
See `docs/developer-docs/database.md` for more detailed instructions on how to set up the database.

- Install the latest version of docker desktop and follow the instructions
- Execute `docker run --name skilltrie_db -e POSTGRES_PASSWORD=postgrespw -e POSTGRES_DB=testdb -p 5432:5432 -d postgres` to initialize the database
- Make sure the database is running using `docker ps` or checking the status in docker desktop

### Backend

You will need to configure the following configuration files before running the backend:

- `backend/appsettings.json`
- `src/.env`

Please follow the following commands and/or instructions to get the backend up and running:

- navigate to the `backend/` folder
- `dotnet restore`
- `dotnet tool install --global dotnet-ef` (Only once when starting the application for the first time)
- `dotnet ef database update`
- `dotnet run`

To seed the database with some initial application data:

- navigate to the `databasePopulator/` folder
- `dotnet run`

### Frontend

To get the dev-server up and running:

- navigate to the `frontend/` folder
- `npm install`
- `npm run dev`

Alternatively, to compile the frontend & let the backend serve it:

- navigate to the `frontend/` folder
- `npm install`
- `npm run build`

### Within the program

To access the admin portion of the system, you need an Admin account. When registering, you register a new User account.
Because of that, an Admin account is created at startup from the `ADMIN_EMAIL` and `ADMIN_PASSWORD` environment variables (both required). For local development you can use for example:
- email: `root@admin.com`
- password: `Root123!`

## How to run the testing environments

Frontend:

- `npm run test`
- `npm run testui` (visual interface will open in browser)

Backend:

- `dotnet test` or run via your IDE's built-in test runner if it has one.

## License

This project was developed by students from the bachelor Computer Science at Utrecht University within the Software Project course, commissioned by Cito.
It is released under the [MIT License](LICENSE).
