# How to populate database with items from databank

### Description of the Database Populator

The database populator is a tool used to set up the database to be usable for the application.
The populator consists of four steps:
-   Truncate (a part of) the current database: This resets certain database tables, you should run this if the database has outdated data. Note that the populator does not update already existing elements of the database, therefore if a cell in a row in the input files gets updated, you might need to truncate in order to parse the change into the database.
-   Parse the input files: All the information in the input files are read and
    translated into rows of specific tables in the database.
    This step also verifies whether a line that needs to be parsed is of valid format.
    Any lines with an incorrect format will not be parsed.
-   Verify the input files: The application makes certain assumptions about the contents of the database. In this step, these constraints are checked. If these are not passed, the program might result in errors. Any future changes to the assumptions about the contents of the database should be included here. Example: we assume the edgelist is acyclic in a directional sense (i.e., A to B to A is invalid).
-   Update the existing database: The input files might have influence on other aspects of the database. In this step, we ensure that the remaining database is still functional. Example: some badges are achieved when completing a topic, and must be created after importing those topics.

### Input file formatting

-   Scopes: This file contains the scopes, including their Id (integer), name (string), and type ("subject", "domain", or "topic"). Scopes function as a tree. Scopes can have any number of parents/ children. To add a context to this structure, the "top" scopes are "subjects" such as "statistics", the "bottom" scopes are "topics" within that subject, and any scopes between those are "domains" which function as a categorisation of those topics in that subject.
-   Scope_membership: This file contains the scope memberships, including the parent Id (integer) and child Id (integer). Since scopes function as a tree, this file defines the relationship between different scopes.
-   Scope_edgelist: This file defines an edgelist between specifically topics (i.e., the knowledge tree). On the home page, this tree will be displayed allowing the user to navigate through the tree from start to finish and does not include domains or subjects.
-   Items: This file contains all information regarding items, including but not limited to the Id (integer) and the file name of the corresponding `.xml` file in the `items` folder.
-   Items_membership: This file contains the item memberships, including the item Id (integer) and scope Id (integer). These tuples define which item(s) belong to which scope(s).
-   Characters & cosmetics: These are not related to the scope architecture.

### Prerequisites

-   Download the QTI items .zip file from the link sent by CITO in Teams.
-   Extract items to `databasePopulator/items`.

-   Download the input files .zip file from the link sent by CITO in Teams.
-   Extract items to `databasePopulator/input`.
-   The files might already be included in the directory, in which case you need to verify whether it needs updating.

Now you should have folders called `items` and `input` in your `databasePopulator` folder
containing several thousand `.xml` items and the input files respectively.

### Run databasePopulator:

-   Make sure you're in the `backend` directory.
-   Run `dotnet ef database update`.
-   Go back to `databasePopulator` directory.
-   Run `dotnet run`.

### Database Migrations

Beware that running `dotnet ef database update` might fail, telling you to fix your database migrations.