# BookStore API

This project is a BookStore API built with ASP.NET Core and PostgreSQL. It provides endpoints to manage books, including features like pagination, search, and authentication.

## Table of Contents

- [BookStore API](#bookstore-api)
  - [Table of Contents](#table-of-contents)
  - [Prerequisites](#prerequisites)
  - [Setup](#setup)
    - [Local Database Setup](#local-database-setup)
  - [Running the Project Locally](#running-the-project-locally)
  - [Running the Project with Docker](#running-the-project-with-docker)
    - [Full Setup with Docker](#full-setup-with-docker)
    - [Environment Variables for Docker](#environment-variables-for-docker)
  - [Running Tests with Docker](#running-tests-with-docker)
  - [Environment Variables](#environment-variables)
  - [API Documentation](#api-documentation)
  - [Design Choices and Trade-offs](#design-choices-and-trade-offs)
    - [Use of ASP.NET Core and PostgreSQL](#use-of-aspnet-core-and-postgresql)
    - [Manual Mapping vs. Automapper](#manual-mapping-vs-automapper)
    - [Error Handling Middleware](#error-handling-middleware)
    - [Environment Variable Configuration](#environment-variable-configuration)
    - [Testing Strategy](#testing-strategy)
    - [Seeded Admin User](#seeded-admin-user)
    - [Docker and Docker Compose](#docker-and-docker-compose)

## Prerequisites

Before you begin, ensure you have the following installed on your machine:

- Running locally
  - [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
  - [PostgreSQL](https://www.postgresql.org/download/)
- Running with Docker
  - [Docker](https://www.docker.com/get-started)
  - [Docker Compose](https://docs.docker.com/compose/install/)

## Setup

### Local Database Setup

1. Install PostgreSQL and create a new database:

    ```sh
    createdb bookstore
    ```

2. Set up the database schema and seed data:

    ```sh
    dotnet ef database update
    ```

## Running the Project Locally

1. Clone the repository:

    ```sh
    git clone https://github.com/Axmouth/bookstore-api.git
    cd bookstore-api
    ```

2. Configure the connection string in `appsettings.json`:

    ```json
    "PostgreSQL": {
        "ConnectionString": "Host=localhost;Database=bookstore;Username=admin;Password=admin123;Port=5432"
    }
    ```

    Or run the docker compose file to automatically set up a database with the default string
    ```bash
    docker-compose -f docker-compose.db-local.yml up --build
    ```

3. Restore dependencies and run the application:

    ```sh
    dotnet restore
    dotnet run --project BookStoreApi
    ```

4. The API will be available at `http://localhost:5000`.

## Running the Project with Docker

### Full Setup with Docker

1. Build and run the Docker containers:

    ```sh
    docker-compose -f docker-compose.full.yml up --build
    ```

2. The API will be available at `http://localhost:5000`.

### Environment Variables for Docker

You can override the default environment variables by setting them before running Docker Compose. For example:

```sh
POSTGRES_DB=localbookstore POSTGRES_USER=localadmin POSTGRES_PASSWORD=localpass docker-compose -f docker-compose.full.yml up --build
```

## Running Tests with Docker

1. Build and run the Docker containers for testing:

    ```sh
    docker-compose -f docker-compose.test.yml up --build
    ```

## Environment Variables

The project uses environment variables to configure the connection strings and other settings. You can set these variables in your environment or in a `.env` file. The following variables are used:

- `ASPNETCORE_ENVIRONMENT`: The environment in which the app is running (e.g., Development, Production).
- `PostgreSQL__ConnectionString`: The connection string for the PostgreSQL database.
- `JwtSettings__Secret`: The secret key used for JWT authentication.
- `JwtSettings__Issuer`: The issuer of the JWT.
- `JwtSettings__Audience`: The audience of the JWT.
- `JwtSettings__TokenLifetime`: The token lifetime in minutes.
- `AdminSettings__AdminEmail`: The admin user's email.
- `AdminSettings__AdminPassword`: The admin user's password.
- `AdminSettings__AdminUsername`: The admin user's username.

## API Documentation

API documentation is available via Swagger. When running the project locally or via Docker, navigate to `http://localhost:5000/swagger/v1/swagger.json` to view the API documentation.

## Design Choices and Trade-offs

### Use of ASP.NET Core and PostgreSQL
ASP.NET Core is a modern, high-performance framework for building web applications and APIs, providing excellent support for dependency injection, middleware, and various other enterprise-grade features. PostgreSQL is chosen for its reliability, robustness, and advanced features as a relational database.

### Manual Mapping vs. Automapper
Instead of using a library like AutoMapper, manual mapping methods (`FromBook`, `ToBook`) are used. This decision is based on:
- **Simplicity**: For a small project, manual mapping is straightforward and easy to understand.
- **Control**: Manual mapping provides explicit control over how objects are transformed, making the code more transparent.
- **Performance**: Avoiding the overhead of an additional library can be beneficial for performance-sensitive applications.

### Error Handling Middleware
A custom middleware is implemented for centralized error handling. This ensures that all exceptions are caught and handled in a consistent manner, improving maintainability and readability of the code.

### Environment Variable Configuration
Environment variables are used to manage configuration settings, allowing the application to be easily configured for different environments (development, testing, production). Docker Compose files are set up to use default values, which can be overridden as needed.

### Testing Strategy
The project primarily uses integration tests to ensure that different components work together as expected. Docker Compose is used to create a test environment that mimics production, ensuring that the tests are reliable and consistent.

### Seeded Admin User
An admin user is seeded into the database during initialization with the following credentials:
- **Email**: `admin@bookstore.com`
- **Username**: `admin`
- **Password**: `Admin@1231`

Currently, there is no functionality to create users via the API. Future enhancements could include user registration and management features. The user's credentials can be overriden with previous mentioned environment variables.

### Docker and Docker Compose
Docker is used to containerize the application, ensuring consistent behavior across different environments. Docker Compose is used to manage multi-container applications, making it easy to set up and run the entire stack (API and database) locally or in a CI/CD pipeline.

Brief Explanation of the Design Choices and Trade-offs
Use of ASP.NET Core and PostgreSQL

The choice of ASP.NET Core was made for its high performance, scalability, and extensive support for modern web development practices. PostgreSQL was selected as the database due to its robustness, advanced features, and strong support for relational data.
Manual Mapping vs. AutoMapper

Manual mapping methods (FromBook, ToBook) were implemented instead of using AutoMapper. This decision was based on:

  - Simplicity: Manual mapping is straightforward for a small project, making it easier to understand and maintain.
  - Control: It provides explicit control over data transformation, ensuring transparency in the codebase.
  - Performance: Avoids the overhead of an additional library, beneficial for performance-sensitive applications.

A custom middleware was created to handle errors uniformly across the application. This approach ensures consistency in error responses and centralizes error handling logic, making the application easier to maintain and debug.
Environment Variable Configuration

Using environment variables for configuration allows the application to be easily adapted to different environments (development, testing, production). Docker Compose files leverage these variables to provide default values, which can be overridden as needed, enhancing flexibility and security.
Integration Testing with Docker

Integration tests were prioritized over unit tests to ensure comprehensive testing of how components interact with each other. Docker Compose is used to replicate the production environment during testing, ensuring that tests are reliable and consistent.
Seeded Admin User

To facilitate immediate use and testing, an admin user is seeded during database initialization. The current design does not include user registration features, which simplifies initial development but limits user management capabilities.
Docker and Docker Compose

Docker is utilized to containerize the application, ensuring consistent behavior across different environments. Docker Compose manages multi-container setups, simplifying the process of running the entire stack locally and in CI/CD pipelines. This choice enhances development efficiency and deployment consistency.