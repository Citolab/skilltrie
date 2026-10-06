# Getting Started: Running PostgreSQL with Docker

This guide will walk you through setting up a PostgreSQL database using Docker — no prior Docker experience needed.

---

## What is Docker?

Docker is a tool that lets you run software in isolated "containers." Think of a container like a self-contained box that includes everything an application needs to run — in this case, a full PostgreSQL database — without you having to install or configure it manually on your machine.

---

## Step 1: Install Docker

If you don't have Docker installed yet:

1. Go to [https://www.docker.com/products/docker-desktop](https://www.docker.com/products/docker-desktop)
2. Download **Docker Desktop** for your operating system (Windows, macOS, or Linux)
3. Run the installer and follow the on-screen instructions
4. Once installed, open Docker Desktop and wait for it to finish starting up

To verify Docker is working, open a terminal and run:

```bash
docker --version
```

You should see something like `Docker version <version-number>` printed out.

---

## Step 2: Start the PostgreSQL Container

Once Docker is running, open your terminal and run the following command:

```bash
docker run --name skilltrie_db -e POSTGRES_PASSWORD=postgrespw -e POSTGRES_DB=testdb -p 5432:5432 -d postgres
```

Here's what each part of that command does:

| Flag | What it does |
|---|---|
| `--name skilltrie_db` | Gives the container a name so you can refer to it easily |
| `-e POSTGRES_PASSWORD=postgrespw` | Sets the database superuser password to `postgrespw` |
| `-e POSTGRES_DB=testdb` | Creates a database called `testdb` automatically on startup |
| `-p 5432:5432` | Connects port `5432` on your machine to port `5432` inside the container (the default PostgreSQL port) |
| `-d` | Runs the container in the background (detached mode) |
| `postgres` | The name of the official PostgreSQL image to download and run |

> **Note:** The first time you run this, Docker will download the PostgreSQL image from the internet. This may take a minute or two.

---

## Step 3: Verify the Container is Running

To confirm your database container is up and running:

```bash
docker ps
```

You should see `skilltrie_db` listed with a status of `Up`.

---

## Connecting to the Database

Use the following credentials to connect from your application or a database client (e.g. Datagrip, pgAdmin):

| Setting | Value |
|---|---|
| **Host** | `localhost` |
| **Port** | `5432` |
| **Database** | `testdb` |
| **Username** | `postgres` |
| **Password** | `postgrespw` |

---

## Useful Docker Commands

Once your container is running, here are a few handy commands to manage it:

```bash
# Stop the database container
docker stop skilltrie_db

# Start it again after stopping
docker start skilltrie_db

# View logs if something seems wrong
docker logs skilltrie_db

# Remove the container entirely (note: this deletes all data)
docker rm -f skilltrie_db
```

---

## Troubleshooting

**"Port 5432 is already in use"**
You may already have PostgreSQL installed locally and running. Stop the local service, or change the left-hand port number (e.g. `-p 5433:5432`) and connect on port `5433` instead.

**"Cannot connect to the Docker daemon"**
Docker Desktop isn't running. Open the Docker Desktop app and wait for it to fully start before trying again.

**Container exits immediately**
Run `docker logs skilltrie_db` to see what went wrong. Often this is a configuration issue with environment variables.

**The database is empty**
Run `dotnet run` in src/backend/databasePopulator to repopulate the database. 