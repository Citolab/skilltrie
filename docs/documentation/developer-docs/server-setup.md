# Server Setup and Development Workflow

This documentation describes how to set up, manage, and work with our project server, including Docker containers, Nginx, GitLab runners, and the CI/CD pipeline.

---

## 1. Server Provisioning

1. **Request a server** – Follow your course or university instructions.  
   - Recommended configuration: Ubuntu server with **at least 2 CPU cores and 4GB RAM**.  
   - For larger workloads, **4 cores and 8GB RAM** is also fine. This is what we used, especially as the frontend tests can be a bit slow on only 4GB. 

2. **Directory structure** – By default, the server may provide a basic file structure. Our setup further uses (from root directory):
```
/var/www/docs # Documentation frontend
/var/www/app-dev # Application frontend
/etc/nginx/conf.d/ # Nginx configuration
/run/secrets/ # Secrets folder for things like DB passwords. 
```

---

## 2. Install Docker

We use Docker to ensure the development environment is consistent across machines and to simplify deployment.

1. Install the following Docker components on your server:
- **Docker Engine**
- **Docker Runtime**
- **Docker Daemon**

2. Follow the official installation instructions for Ubuntu [here](https://docs.docker.com/engine/install/ubuntu/).

3. Test your installation:

```bash
docker --version
docker compose version
```

## 3. Nginx Configuration
We serve frontend applications via Nginx, mapping domains to specific directories:
```
Documentation frontend → /var/www/docs
App frontend (for development) → /var/www/app-dev
```

Nginx acts as a reverse proxy, forwarding requests to backend containers using the ports defined in the Docker Compose setup.
Steps to configure:
Install Nginx:
```
sudo apt update
sudo apt install nginx
```

Next, follow the nginx documentation to set up your nginx reverse proxy. Make sure to hook everything up to the correct ports the UU server provided you, and use the same ports as defined in your docker compose file. 

If you ever make changes to the nginx configuration (for us, located at ```etc/nginx/conf.d/```), make sure to run
```sudo systemctl reload nginx```

## 4. Docker Containers Setup
We run multiple containers on the server for the backend, database and gitlab runners.
Components:
```
Backend – Runs in its own Docker container.
Database – Runs in a separate Docker container.
GitLab runners – Two containers:
    - One runs shell scripts
    - One runs Docker commands
```

## 5. GitLab Runners
GitLab runners handle CI/CD jobs on the server. They must be configured on the server, not your local machine.
Setup Steps:
Run GitLab runners in Docker containers.
Two runners are configured:
```
Shell runner – Executes shell scripts.
Docker runner – Executes Docker-based jobs.
```

In the settings of your gitlab project, a full guide to setting up gitlab runners can be found. Follow this guide, and make sure to give your ci/cd jobs the correct tags to differentiate between shell runners and docker runners. Generally, shell runners are used for deploying, and docker runners for testing and validation. 

# 6. Working with the Server

Starting the project should be managed using the CI/CD pipeline. Stopping the project can be done manually by calling ```docker stop <container-name>```.

The site can be reached at the address:
```
Documentation: http://<your-server>/docs/
Application: http://<your-server>/app-dev/
```
We suggested you also secure this to https using certbot and perhaps securing the documentation and application using some authentication handler. 

### Logs
Backend logs can be ran with ```docker compose logs -f backend``` while database logs can be ran using
```docker compose logs -f db```

# 7. CI/CD Pipeline
The original GitLab pipeline (.gitlab-ci.yml, not included in this repository) defined jobs for building, testing, and deploying containers.

Jobs typically include:
- Copying sensitive files from the runner’s home directory.
- Building backend, frontend, and database containers.
- Running tests.
- Building autodocs
- Validating merge requests based on source

The CI/CD pipeline, and all dockerfiles used inside of it, can be found in the ci/ folder in the root directory. 
