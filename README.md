# Review Proxy

Review Proxy is a microservices-based application built with a .NET 10 backend and a Vue 3 frontend. 

## Project Structure

- **`src/frontend/`**: The web client application. Built with Vue 3, Vite, PrimeVue, and Tailwind CSS. Managed by Bun.
- **`src/gateway/`**: The API Gateway acting as a single entry point for backend services (using YARP).
- **`src/services/identity/`**: The Identity microservice responsible for user authentication (including GitHub OAuth) and profile management. Backed by its own PostgreSQL database.
- **`src/services/repository/`**: The Repository microservice handling repository-related operations and data. Backed by its own PostgreSQL database.

## Architecture

The backend consists of independently deployable microservices and a gateway:
- **Gateway**: `localhost:8000`
- **Identity Service**: `localhost:5246`
- **Repository Service**: `localhost:5247`
- **Identity DB** (PostgreSQL): `localhost:5433`
- **Repo DB** (PostgreSQL): `localhost:5432`

## Prerequisites

To run this project locally, ensure you have the following installed:
- [Docker](https://docs.docker.com/get-docker/) and [Docker Compose](https://docs.docker.com/compose/install/)
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Bun](https://bun.sh/) (for frontend development)

## Running the Application Locally

### Backend (Docker Compose)

A convenience script (`dev.sh`) is provided in the root directory to manage the backend services via Docker Compose. The services run in `.NET Development` environment mode.

Start all backend services:
```bash
./dev.sh up
```

Stop all services:
```bash
./dev.sh down
```

Rebuild and start a specific service after making code changes (e.g., to the gateway):
```bash
./dev.sh build gateway
```

Tail logs for a specific service:
```bash
./dev.sh logs repository
```

To see all available commands, run:
```bash
./dev.sh
```

### Frontend

The frontend is built with Vue and Vite, using Bun for package management. 

Navigate to the frontend directory:
```bash
cd src/frontend
```

Install dependencies:
```bash
bun install
```

Start the development server:
```bash
bun run dev
```

## Authentication Setup

The Identity service uses GitHub OAuth for authentication. You will need to set up a GitHub OAuth application and provide your `ClientId` and `ClientSecret` to the Identity service configuration (e.g., via User Secrets, `appsettings.Development.json`, or environment variables).

### Environment Variables
- `GitHub__ClientId`: Your GitHub OAuth application client ID
- `GitHub__ClientSecret`: Your GitHub OAuth application client secret
