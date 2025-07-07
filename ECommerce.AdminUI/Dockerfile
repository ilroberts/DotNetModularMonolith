# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files for restore
COPY ModularMonolith.sln ./
COPY ECommerce.AdminUI/*.csproj ./ECommerce.AdminUI/

# Restore packages
RUN dotnet restore ECommerce.AdminUI/ECommerce.AdminUI.csproj

# Copy everything else and build
COPY ECommerce.AdminUI/. ./ECommerce.AdminUI/

# Build and publish
RUN dotnet publish ECommerce.AdminUI/ECommerce.AdminUI.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create a non-root user
RUN groupadd -r appgroup && useradd -r -g appgroup appuser
USER appuser

# Copy the published app
COPY --from=build /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080

# Expose the port
EXPOSE 8080

# Start the application
ENTRYPOINT ["dotnet", "ECommerce.AdminUI.dll"]
