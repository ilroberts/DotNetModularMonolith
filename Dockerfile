# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and restore as distinct layers
COPY ModularMonolith.sln ./
COPY ECommerceApp/*.csproj ./ECommerceApp/
COPY ECommerce.Common/*.csproj ./ECommerce.Common/
COPY ECommerce.Contracts/*.csproj ./ECommerce.Contracts/
COPY ECommerce.BusinessEvents/*.csproj ./ECommerce.BusinessEvents/
COPY ECommerce.Modules.Customers/*.csproj ./ECommerce.Modules.Customers/
COPY ECommerce.Modules.Orders/*.csproj ./ECommerce.Modules.Orders/
COPY ECommerce.Modules.Orders.Tests/ECommerce.Modules.Orders.Tests.csproj ./ECommerce.Modules.Orders.Tests/
COPY tests/ECommerce.ArchitectureTests/ECommerce.ArchitectureTests.csproj ./tests/ECommerce.ArchitectureTests/
COPY ECommerce.BusinessEvents.Tests/ECommerce.BusinessEvents.Tests.csproj ./ECommerce.BusinessEvents.Tests/
COPY ECommerce.Modules.Products/*.csproj ./ECommerce.Modules.Products/
COPY ECommerceApp/*.csproj ./ECommerceApp/
RUN dotnet restore

# Copy everything else and build
COPY . .
RUN dotnet publish ECommerceApp/ECommerceApp.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development
EXPOSE 8080

ENTRYPOINT ["dotnet", "ECommerceApp.dll"]
