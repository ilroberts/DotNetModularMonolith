# Modular Monolith E-Commerce Application

## Overview

**ECommerceApp** is a modular monolith built with **.NET 8** that serves as an example of how to build an e-commerce system using a modular architecture. The application is structured into three primary modules:

- **Orders**: Manages customer orders and interacts with the product catalog to create orders.
- **Products**: Handles the product catalog, exposing product information.
- **Customers**: Manages customer information.

### Key Features

- **Modular Monolith Architecture**: The application is divided into modules (Orders, Products, Customers) with loose coupling.
- **Dependency Injection**: The modules interact using shared interfaces for decoupled communication.
- **Shared Contracts**: Contracts are placed in a separate project to avoid circular dependencies between modules.
- **Logging**: Integrated logging using **ILogger**.
- **Swagger UI**: API documentation and testing via **Swagger**.

## Table of Contents

- [Modular Monolith E-Commerce Application](#modular-monolith-e-commerce-application)
  - [Overview](#overview)
    - [Key Features](#key-features)
  - [Table of Contents](#table-of-contents)
  - [Project Structure](#project-structure)
  - [Requirements](#requirements)
  - [Setup and Installation](#setup-and-installation)
  - [Usage](#usage)
    - [Orders Module](#orders-module)
    - [Products Module](#products-module)
    - [Customers Module](#customers-module)
  - [Logging](#logging)
  - [Swagger Integration](#swagger-integration)
  - [Contributing](#contributing)
    - [Steps to Contribute](#steps-to-contribute)
  - [License](#license)
  - [Running with Kubernetes and Skaffold](#running-with-kubernetes-and-skaffold)
    - [Prerequisites](#prerequisites)
    - [Steps](#steps)
    - [Notes](#notes)

## Project Structure

The project follows a modular monolith structure, where each module is self-contained but part of a single application.

```plaintext
ECommerceApp/
│
├── ECommerce.Contracts/                # Shared interfaces and DTOs
│   ├── Interfaces/
│   ├── DTOs/
│
├── ECommerce.Modules.Orders/           # Orders module
│   ├── Services/
│   ├── Endpoints/
│   ├── Extensions/
│
├── ECommerce.Modules.Products/         # Products module
│   ├── Services/
│   ├── Endpoints/
│   ├── Extensions/
│
├── ECommerce.Modules.Customers/        # Customers module
│   ├── Services/
│   ├── Endpoints/
│   ├── Extensions/
│
└── Program.cs                          # Application entry point
```

## Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Visual Studio](https://visualstudio.microsoft.com/) or any other C# IDE
- [SQL Server](https://www.microsoft.com/en-us/sql-server) or other databases (optional, based on your implementation)

## Setup and Installation

Follow these steps to set up the project:

1. **Clone the repository**:

   ```bash
   git clone https://github.com/yourusername/ECommerceApp.git
   cd ECommerceApp
   ```

2. **Install dependencies**:

   Run the following command to restore all required packages:

   ```bash
   dotnet restore
   ```

3. **Build the project**:

   Build the project using the following command:

   ```bash
   dotnet build
   ```

4. **Run the application**:

   Start the application:

   ```bash
   dotnet run
   ```

5. **Access the API**:

   Once the app is running, you can access the API at:

   ```bash
   http://localhost:5000
   ```

6. **Swagger Documentation**:

   Visit the Swagger UI for API documentation:

   ```bash
   http://localhost:5000/swagger
   ```

## Usage

### Orders Module

The Orders module allows for creating, retrieving, and managing customer orders. It depends on the **Products** module for product data.

Key Endpoints:

- `POST /orders`: Create a new order
- `GET /orders`: Get all orders
- `GET /orders/{id}`: Get order by ID

### Products Module

The Products module manages the product catalog and exposes product data to other modules via a shared interface.

Key Endpoints:

- `GET /products`: Get all products
- `GET /products/{id}`: Get product by ID

### Customers Module

The Customers module handles customer data and services.

Key Endpoints:

- `GET /customers`: Get all customers
- `GET /customers/{id}`: Get customer by ID

## Logging

Logging is integrated using `ILogger`. Example of usage:

```csharp
public async Task CreateOrderAsync(Guid customerId, List<OrderItem> items)
{
    _logger.LogInformation("Creating order for customer {CustomerId}", customerId);
    // business logic here
    _logger.LogInformation("Order created for customer {CustomerId}", customerId);
}
```

Logs are written to the console, and you can extend this to use other logging providers (e.g., file logging, cloud logging).

## Swagger Integration

Swagger is integrated to allow easy API documentation and testing. To access Swagger, go to:

```bash
http://localhost:5000/swagger
```

You can interact with all the API endpoints directly from the Swagger UI.

## Contributing

If you'd like to contribute, feel free to fork the repository and submit a pull request. Issues and feature requests are welcome.

### Steps to Contribute

1. Fork the repository.
2. Create a new branch (`git checkout -b feature-branch`).
3. Commit your changes (`git commit -m 'Add new feature'`).
4. Push to the branch (`git push origin feature-branch`).
5. Create a pull request.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Running with Kubernetes and Skaffold

You can run the application in Kubernetes using [Skaffold](https://skaffold.dev/), which automates the build, push, and deploy process for local development.

### Prerequisites

- Docker (or another supported container runtime)
- Kubernetes cluster (e.g., minikube, Docker Desktop, or a remote cluster)
- Skaffold installed ([installation guide](https://skaffold.dev/docs/install/))

### Steps

1. **Start your Kubernetes cluster** (if using minikube):

   ```bash
   minikube start
   ```

2. **Run Skaffold** from the project root:

   ```bash
   skaffold dev
   ```

   This will build the Docker image, deploy the manifests in `ECommerceApp/k8s/`, and watch for changes.

3. **Access the Application:**

   - Get the ingress IP:

     ```bash
     kubectl get ingress
     ```

   - Open `http://<INGRESS-IP>/swagger` in your browser to access the Swagger UI.
   - If using minikube, you may need to run `minikube tunnel` to expose the ingress controller.

### Notes

- The manifests for deployment, service, and ingress are located in `ECommerceApp/k8s/`.
- The Dockerfile sets `ASPNETCORE_ENVIRONMENT=Development` so Swagger is enabled by default.
