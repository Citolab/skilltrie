# Environment Variables

All the different environment variables are gathered in one `.env` file placed inside `src/`. This makes it more organized and easier to apply changes without having to search the codebase.

## What does it look like
Environment variables for the frontend are prefixed with `VITE`, the other environment variables are used in the backend.


| Key | Clarification |
| --- | ---           |
| **Backend** ||
| BACKEND_DB_HOST | The address on which the database is hosted, e.g. `localhost` |
| BACKEND_DB_PORT | The port that the database is running |
| BACKEND_DB_DATABASENAME | Name of the database |
| BACKEND_DB_AUTOMIGRATE | Boolean value to automatically migrate |
| BACKEND_DB_USERNAME | Username of the Postgres database|
| POSTGRES_PASSWORD | Password use for the database |
| POSTGRES_DB | Name of the database |
|  |  |
| **AspNet** ||
| BACKEND_ENVIRONMENT | Variable to indicate what environment the appliation is running on, e.g. `Development`|
| ASPNETCORE_URLS | URL on which the backend is running |
| ADMIN_EMAIL | Email for default admin user |
| ADMIN_PASSWORD | Password for default admin user |
|||
| **AI API** || 
| BACKEND_AI_ENDPOINT | Endpoint on which the AI model is hosted |
| BACKEND_AI_DEPLOYMENT_NAME | The name of the AI model used |
| BACKEND_AI_KEY | The API key for the model |
| BACKEND_AI_VERSION | The version of the model |
|||
| **Frontend** | |
| VITE_BACKEND_URL | The URL pointing to the backend|
| VITE_BASE_PATH | The base path at which the application is running, e.g. `/appdev/` |
| VITE_ENVIRONMENT | The environment that the frontend is running in, e.g. `development`| 
| **Posthog** || 
| VITE_POSTHOG_PROJECT_TOKEN | The frontend project token from posthog|
| VITE_POSTHOG_HOST | The URL at which the frontend posthog application is hosted |
| PostHog__ProjectApiKey | The backend project token from posthog |
| PostHog__HostUrl | The URL at which the backend posthog application is hosted |

