# Back-End

## .NET version

.NET 8 (.NET 8 uses C# language version 12)

If you have the .NET SDK for version 8 installed, you should be all good.

## Running:

1. `cd backend/API` if you haven't done so already
2. `dotnet restore` if needed to install packages from `API.csproj`
3. `dotnet run` the server should be configured to start running on localhost:5432

## Ports:

Make sure your ports are correct.
\
All of the ports can be found in the `.env` file located in the src folder. 
\
The backend should be set to 5433, the frontend to 5173 and the database to 5432

## Database:

### Setup:

Download PostgreSQL (version 17 is used, lower is most likely fine as well)

1. Make sure Docker (<a href = https://docs.docker.com/desktop/>Desktop</a>) is correctly installed
2. While Docker (Desktop) is running, run the following command <br/>```docker run --name skilltrie_db -e POSTGRES_PASSWORD=postgrespw -e POSTGRES_DB=testdb -p 5432:5432 -d postgres``` <br> 
so that the connection variables are set up properly your connection string in `appsettings.Development.json` or `appsettings.json` that is being loaded in `Program.cs`.
3. In the folder `backend` run `dotnet tool install --global dotnet-ef` to install the Entity Framework Core Command Line Interface tool for the next steps.
4. If something goes wrong later, you might need to stop the `postgresql-x64-17` service, for Windows, 
   press `Windows+r` and enter `services.msc`. Then scroll to the `postgresql-x64-17` service, right click it and stop it. You may also want to change the startup type of the service to manual, so that it does not start every time on startup. 

### Modifying:

1. Make changes to database models in the `Models` folder
2. Include possible new tables in `Models/AppDbContext.cs`
3. Run `dotnet ef migrations add <name>` to generate files in the `Migrations` folder representing the changes to the database
4. Run `dotnet ef database update` to apply migrations to your actual database. Other people who pull your code will also need to run this command.

## How to run the testing environment

- `dotnet test` or run via your IDE's built-in test runner if it has one.