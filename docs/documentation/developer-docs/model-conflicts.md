
# Conflicts of models in the backend

It is possible that on a feature branch some changes to the model are made that have a conflict with incoming changes from for example `dev`

With Git it is standard to **overwrite** the current changes with the incoming changes. This prevents the risk of accidentally breaking someone elses code.

# Fix Migrations

If you haven't committed any migrations to your branch yet, you generally don't need to worry, but if you have and you're running into problems, follow these steps:

1. Downgrade your database to an earlier version using `dotnet ef database update`, or drop your database. Then, remove the migrations you’ve committed to your branch. If you have data in your tables, you may also need to remove it by truncating the tables or executing `delete from [table]`.
2. Overwrite your current ModelSnapshot with the one coming from `dev`. This keeps track of the database’s current structure and determines what should go into a new migration. So never delete migrations without rolling back the model.
3. Fix your current model if it has conflicts and create a new migration with `dotnet ef migrations add [name]`
4. Commit your changes and inform the people working on the same branch that they need to downgrade their database, pull your changes, and then update the database again. 

This is indeed a hassle, but unfortunately, there’s no better way.