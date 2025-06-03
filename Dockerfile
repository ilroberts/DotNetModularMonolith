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
COPY ECommerce.Modules.Products/*.csproj ./ECommerce.Modules.Products/
COPY ECommerceApp/*.csproj ./ECommerceApp/
COPY ECommerce.DatabaseMigrator/*.csproj ./ECommerce.DatabaseMigrator/
RUN dotnet restore

# Copy everything else and build
COPY . .
RUN dotnet publish ECommerceApp/ECommerceApp.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app
COPY --from=build /app/publish .

# Create a non-root user and set permissions
RUN addgroup -S appgroup && adduser -S appuser -G appgroup \
    && chown -R appuser:appgroup /app
USER appuser

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development
EXPOSE 8080

ENTRYPOINT ["dotnet", "ECommerceApp.dll"]
