# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Key Commands

### Building and Running

```bash
# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run the application
dotnet run --project ECommerceApp/ECommerceApp.csproj

# Publish the application
dotnet publish ECommerceApp/ECommerceApp.csproj -c Release -o ./publish
```

### Testing

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test ECommerce.Modules.Orders.Tests/ECommerce.Modules.Orders.Tests.csproj
dotnet test ECommerce.BusinessEvents.Tests/ECommerce.BusinessEvents.Tests.csproj
dotnet test tests/ECommerce.ArchitectureTests/ECommerce.ArchitectureTests.csproj

# Run a single test (example)
dotnet test --filter "FullyQualifiedName=ECommerce.Modules.Orders.Tests.Services.OrderServiceTests.CreateOrder_ShouldReturnOrder_WhenOrderIsValid"
```

### Docker and Kubernetes

```bash
# Build Docker image
docker build -t modularmonolith .

# Run with Skaffold (local Kubernetes development)
skaffold dev

# Switch Docker environment (if needed)
./switch-docker-env.sh
```

## Architecture Overview

This is a .NET 8 modular monolith application structured around domain-driven design principles. The application demonstrates an e-commerce system with a modular architecture that maintains separation of concerns while keeping everything in a single deployable unit.

### Core Architecture Principles

1. **Modular Structure**: The application is divided into separate modules (Orders, Products, Customers, BusinessEvents), each encapsulated in its own project with clear boundaries.

2. **Loose Coupling**: Modules communicate through well-defined interfaces in the `ECommerce.Contracts` project, preventing direct dependencies between modules.

3. **Shared Nothing Architecture**: Each module has its own DbContext and domain models, avoiding shared persistence.

4. **Architecture Enforcement**: The solution includes architecture tests that enforce design rules like preventing direct module dependencies.

### Module Structure

Each module follows a similar structure:
- `Domain/`: Contains entity models
- `Endpoints/`: Contains API endpoint definitions 
- `Services/`: Contains business logic and implementation of interfaces
- `Persistence/`: Contains DbContext and database configuration
- `{Module}Module.cs`: Entry point with dependency registration

### Inter-Module Communication

Modules communicate through interfaces defined in `ECommerce.Contracts`, which are implemented by service classes in each module. This allows modules to call each other's functionality without direct references.

### Testing Approach

- Unit tests per module (e.g., `ECommerce.Modules.Orders.Tests`)
- Architecture tests to enforce design rules (`ECommerce.ArchitectureTests`)
- Pact Consumer tests are located in ECommerce.Pact.ConsumerTests

### Deployment

The application is containerized using Docker and can be deployed to Kubernetes using Skaffold for local development. The application exposes a REST API with Swagger documentation enabled in development environments.

## Skills

| Skill | Location | When to use |
|---|---|---|
| conventional-commits | `.github/conventional-commits/SKILL.md` | Use when creating git commit messages |
