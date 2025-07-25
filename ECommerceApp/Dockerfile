# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy only the necessary project files for dependency restoration
COPY ECommerceApp/*.csproj ./ECommerceApp/
COPY ECommerce.Common/*.csproj ./ECommerce.Common/
COPY ECommerce.Contracts/*.csproj ./ECommerce.Contracts/
COPY ECommerce.BusinessEvents/*.csproj ./ECommerce.BusinessEvents/
COPY ECommerce.Modules.Customers/*.csproj ./ECommerce.Modules.Customers/
COPY ECommerce.Modules.Orders/*.csproj ./ECommerce.Modules.Orders/
COPY ECommerce.Modules.Products/*.csproj ./ECommerce.Modules.Products/

# Restore dependencies
RUN dotnet restore ECommerceApp/ECommerceApp.csproj

# Copy only the necessary project source files
COPY ECommerceApp/ ./ECommerceApp/
COPY ECommerce.Common/ ./ECommerce.Common/
COPY ECommerce.Contracts/ ./ECommerce.Contracts/
COPY ECommerce.BusinessEvents/ ./ECommerce.BusinessEvents/
COPY ECommerce.Modules.Customers/ ./ECommerce.Modules.Customers/
COPY ECommerce.Modules.Orders/ ./ECommerce.Modules.Orders/
COPY ECommerce.Modules.Products/ ./ECommerce.Modules.Products/

# Build the application
RUN dotnet publish ECommerceApp/ECommerceApp.csproj -c Release -o /app/publish

# Runtime stage
FROM --platform=linux/arm64 mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development
EXPOSE 8080

ENTRYPOINT ["dotnet", "ECommerceApp.dll"]
