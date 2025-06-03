# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files for restore
COPY ModularMonolith.sln ./
COPY ECommerce.DatabaseMigrator/*.csproj ./ECommerce.DatabaseMigrator/
COPY ECommerce.Common/*.csproj ./ECommerce.Common/
COPY ECommerce.Contracts/*.csproj ./ECommerce.Contracts/
COPY ECommerce.BusinessEvents/*.csproj ./ECommerce.BusinessEvents/
COPY ECommerce.Modules.Customers/*.csproj ./ECommerce.Modules.Customers/
COPY ECommerce.Modules.Orders/*.csproj ./ECommerce.Modules.Orders/
COPY ECommerce.Modules.Products/*.csproj ./ECommerce.Modules.Products/

# Restore packages
RUN dotnet restore ECommerce.DatabaseMigrator/ECommerce.DatabaseMigrator.csproj

# Copy everything else and build
COPY ECommerce.Common/. ./ECommerce.Common/
COPY ECommerce.Contracts/. ./ECommerce.Contracts/
COPY ECommerce.BusinessEvents/. ./ECommerce.BusinessEvents/
COPY ECommerce.Modules.Customers/. ./ECommerce.Modules.Customers/
COPY ECommerce.Modules.Orders/. ./ECommerce.Modules.Orders/
COPY ECommerce.Modules.Products/. ./ECommerce.Modules.Products/
COPY ECommerce.DatabaseMigrator/. ./ECommerce.DatabaseMigrator/

# Build and publish
RUN dotnet publish ECommerce.DatabaseMigrator/ECommerce.DatabaseMigrator.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime
WORKDIR /app

# Install PostgreSQL client for healthchecks (optional)
#RUN apk --no-cache add postgresql-client

# Create a non-root user
RUN addgroup -S appgroup && adduser -S appuser -G appgroup
USER appuser

# Copy the published app
COPY --from=build /app/publish .

# Set the entrypoint
ENTRYPOINT ["dotnet", "ECommerce.DatabaseMigrator.dll"]
