# Review Proxy

Review Proxy is a microservices platform designed to streamline the management and auditing of GitHub repository issues. It allows repository owners to deposit their projects and assign auditors to review issues on their repository, ensuring a structured audit workflow, approved issues can be post automatically to GitHub issue.

## System Architecture

The application follows a distributed microservices pattern:

*   **API Gateway**: The central entry point using YARP to route traffic to backend services.
*   **Identity Service**: Handles user authentication and profiles through GitHub OAuth.
*   **Repository Service**: Manages repository registration and metadata storage.
*   **Issue Service**: Tracks repository issues and their respective audit statuses.
*   **Syncing Service**: Interfaces with the GitHub API to keep local data synchronized.
*   **Communication**: Services communicate asynchronously using RabbitMQ and MassTransit.
*   **Data Storage**: Each microservice utilizes its own PostgreSQL instance to ensure data isolation.

## User Roles and Permissions

*   **Owners**: Users who register repositories in the system. They have permission to manage repository settings and assign auditors to their projects.
*   **Auditors**: Specialized users assigned to repositories. They are responsible for reviewing issues and updating audit statuses.
*   **Registered Users**: Any user authenticated via GitHub who can browse repositories and view audit progress as well as post new issue to deposited repository.

## Technology Stack

*   **Backend**: .NET 10, ASP.NET Core, Entity Framework Core, YARP, MassTransit.
*   **Frontend**: Vue 3, TypeScript, Pinia, PrimeVue, Tailwind CSS.
*   **Infrastructure**: Docker, Docker Compose, RabbitMQ.
*   **Database**: PostgreSQL.
*   **Package Management**: Bun for frontend dependencies and .NET CLI for backend.

## GitHub Configuration

Review Proxy uses a single GitHub App to handle both user authentication (OAuth) and repository interactions.

### GitHub App Setup
1.  **Basic Information**: Set your Homepage URL (e.g. `http://localhost:3000`).
2.  **Identifying and Authorizing Users**:
    *   **Callback URL**: Set this to `http://localhost:8000/api/sync/signin-github` (or your production gateway URL).
    *   **Request user authorization (OAuth) during installation**: Enable this checkbox.
3.  **Permissions**:
    *   **Issues**: Read and write.
    *   **Metadata**: Read-only (mandatory).
4.  **Private Key**: Generate a private key and download the `.pem` file to your host machine.
5.  **Secrets**: Generate a Client Secret.

## Installation and Setup

### Prerequisites
Ensure you have .NET 10 SDK, Docker, and Bun installed on your machine.

### Environment Setup
Create a `.env` file in the root directory by copying `sample.env`. Use the credentials from your single GitHub App for all fields:
*   `GitHub__ClientId`: Your GitHub App Client ID.
*   `GitHub__ClientSecret`: Your GitHub App Client Secret.
*   `GitHub__AppId`: Your GitHub App ID.
*   `GitHub__AppSlug`: Your GitHub App slug.
*   `GitHub__PrivateKeyPath`: The path to your `.pem` file.

## Production Considerations

When moving beyond a local development environment, keep the following in mind:

*   **Security**: Ensure all public endpoints (Gateway and Frontend) are served over HTTPS. Update your GitHub App Callback URL and Homepage URL accordingly.
*   **Secrets Management**: Do not store `.env` files or `.pem` keys in source control. Use a secure vault or environment injection provided by your hosting platform.
*   **Database Reliability**: Configure managed PostgreSQL instances or ensure robust backup strategies for the Docker volumes.
*   **Scaling**: The microservices can be scaled independently. Ensure RabbitMQ is configured with high availability if needed.
*   **Monitoring**: Implement centralized logging and health checks for each service to monitor the distributed system effectively.

## How to Run (Development)

The project includes a utility script to manage the development environment.

1.  **Start the system**:
    ```bash
    ./scripts/dev.sh up
    ```
2.  **Access the application**:
    Open your browser and navigate to `http://localhost:3000`.
3.  **View logs**:
    If you need to troubleshoot, use:
    ```bash
    ./scripts/dev.sh logs [service-name]
    ```
4.  **Stop the system**:
    ```bash
    ./scripts/dev.sh down
    ```
