# Modular Monolith E-Commerce Application

## Overview

**ECommerceApp** is a modular monolith built with **.NET 8** that demonstrates a scalable, maintainable e-commerce system using a modular architecture. The solution is organized into several projects and modules, each with a clear responsibility.

### Key Features

- **Modular Monolith Architecture**: Orders, Products, Customers, and Business Events modules, each with their own domain, endpoints, persistence, and services.
- **Admin UI**: ASP.NET Core Razor Pages frontend for administration, with integration and unit tests.
- **Integration & Unit Testing**: Comprehensive test projects for API modules and Admin UI, using in-memory databases and HTTP stubbing.
- **Dependency Injection**: All modules and services use DI for decoupled communication.
- **Shared Contracts**: DTOs and interfaces in a separate project to avoid circular dependencies.
- **Logging & Observability**: Integrated logging and OpenTelemetry support.
- **Swagger UI**: API documentation and testing via Swagger.
- **Database Migrations**: Dedicated migrator project for applying EF Core migrations.
- **Docker & Kubernetes**: Dockerfiles and k8s manifests for containerized deployment.

## Table of Contents

- [Overview](#overview)
- [Project Structure](#project-structure)
- [Requirements](#requirements)
- [Setup and Installation](#setup-and-installation)
- [Usage](#usage)
- [Testing](#testing)
- [Logging & Observability](#logging--observability)
- [Contributing](#contributing)

## Project Structure

```
ModularMonolith/
├── ECommerceApp/                       # Main API host (wires up modules)
├── ECommerce.Modules.Orders/           # Orders module (domain, endpoints, persistence, services)
├── ECommerce.Modules.Products/         # Products module (domain, endpoints, persistence, services)
├── ECommerce.Modules.Customers/        # Customers module (domain, endpoints, persistence, services)
├── ECommerce.BusinessEvents/           # Business events module
├── ECommerce.Contracts/                # Shared DTOs and interfaces
├── ECommerce.Common/                   # Shared result types, utilities
├── ECommerce.AdminUI/                  # ASP.NET Core Razor Pages admin UI
├── ECommerce.AdminUI.IntegrationTests/ # Integration tests for Admin UI (HTTP stubbing)
├── ECommerce.AdminUI.Tests/            # Unit tests for Admin UI
├── ECommerceApp.IntegrationTests/      # Integration tests for API modules (in-memory DB)
├── ECommerce.Modules.Orders.Tests/     # Unit tests for Orders module
├── ECommerce.BusinessEvents.Tests/     # Unit tests for Business Events module
├── ECommerce.DatabaseMigrator/         # Standalone EF Core migration runner
├── docs/                               # Architecture decision records, Swagger, etc.
├── README.md
└── ...
```

## Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Docker (for running containers and database)
- (Optional) Kubernetes (for k8s manifests)

## Setup and Installation

1. **Clone the repository**
2. **Restore dependencies**
   ```sh
   dotnet restore
   ```
3. **Apply database migrations**
   ```sh
   dotnet run --project ECommerce.DatabaseMigrator
   ```
4. **Run the application**
   ```sh
   dotnet run --project ECommerceApp
   ```
5. **Run the Admin UI**
   ```sh
   dotnet run --project ECommerce.AdminUI
   ```

## Usage

- **API Endpoints**: Available at `/orders`, `/products`, `/customers`, `/businessevents` (see Swagger UI for details)
- **Admin UI**: Navigate to the running Admin UI app for product, order, and customer management
- **Swagger UI**: Visit `/swagger` on the API host

## Testing

- **Integration Tests**:
  - `ECommerceApp.IntegrationTests`: In-memory DB integration tests for API modules
  - `ECommerce.AdminUI.IntegrationTests`: HTTP-stubbed integration tests for Admin UI services
- **Unit Tests**:
  - `ECommerce.AdminUI.Tests`, `ECommerce.Modules.Orders.Tests`, `ECommerce.BusinessEvents.Tests`
- **Run all tests**:
  ```sh
  dotnet test
  ```

## Logging & Observability

- Logging via `ILogger` throughout all modules
- OpenTelemetry support in Admin UI
- API and Admin UI log to console by default

## Contributing

Contributions are welcome! Please see the `docs/` folder for architecture decisions and contribution guidelines.
