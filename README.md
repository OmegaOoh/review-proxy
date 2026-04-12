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

## Authentication & GitHub App Setup

### 1. GitHub OAuth App
The Identity service uses GitHub OAuth for authentication. Create a GitHub OAuth application and provide your `ClientId` and `ClientSecret` to the Identity service configuration.

### 2. GitHub App (Required for Syncing)
To sync issues and perform actions on repositories, you must create a GitHub App:
1.  **Permissions**:
    - **Issues**: Read & Write
    - **Metadata**: Read-only (required for all apps)
2.  **Configuration**:
    - Provide the `AppId`, `ClientId`, `ClientSecret`, and the `PrivateKey` (.pem file path) to the `syncing` service configuration.
    - Set the `AppSlug` (found in your GitHub App's "Public link" setting) in the `syncing` service configuration. **Important**: An incorrect slug will result in a 404 error on the installation page.
3.  **Installation**:
    - Users must install the app on their account or specific repositories before depositing them in ReviewProxy.
    - The application provides a direct link to the installation page in the "Deposit Repository" modal.

### Environment Variables
- `GitHub__ClientId`: Your GitHub App/OAuth client ID
- `GitHub__ClientSecret`: Your GitHub App/OAuth client secret
- `GitHub__AppId`: Your GitHub App ID
- `GitHub__AppSlug`: Your GitHub App slug (e.g. `review-proxy`)
- `GitHub__PrivateKeyPath`: Path to your GitHub App private key `.pem` file
