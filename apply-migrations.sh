#!/bin/bash

# Script to apply all EF Core migrations for the Modular Monolith application
# This script applies migrations for all modules in the correct order

set -e  # Exit on any error

echo "🔧 Applying EF Core Migrations for Modular Monolith"
echo "=================================================="

# Change to the project directory
cd "$(dirname "$0")"

# Function to apply migration for a specific module
apply_migration() {
    local project=$1
    local module_name=$2
    local context=$3

    echo ""
    echo "📦 Applying migration for $module_name..."
    echo "   Project: $project"
    echo "   Context: $context"

    # Apply the migration with specific context
    dotnet ef database update --project "$project" --startup-project ECommerceApp --context "$context" --verbose

    if [ $? -eq 0 ]; then
        echo "✅ $module_name migration applied successfully"
    else
        echo "❌ Failed to apply $module_name migration"
        exit 1
    fi
}

# Check if dotnet is available
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET CLI is not installed or not in PATH"
    exit 1
fi

# Check if the solution file exists
if [ ! -f "ModularMonolith.sln" ]; then
    echo "❌ ModularMonolith.sln not found. Please run this script from the solution root directory."
    exit 1
fi

echo "🔍 Checking connection to database..."

# Test database connection by trying to get context info
if ! dotnet ef dbcontext info --project ECommerce.Modules.Customers --startup-project ECommerceApp --context CustomerDbContext &> /dev/null; then
    echo "❌ Cannot connect to database. Please ensure:"
    echo "   1. PostgreSQL is running"
    echo "   2. Connection string is configured correctly"
    echo "   3. Database exists and is accessible"
    echo ""
    echo "Current connection string points to: localhost:63441"
    echo "You can check if PostgreSQL is running with: netstat -an | grep 63441"
    exit 1
fi

echo "✅ Database connection successful"

echo ""
echo "🚀 Starting migration process..."

# Apply migrations in order
# Note: The order matters due to potential dependencies between modules

# 1. Business Events (foundational event tracking)
apply_migration "ECommerce.BusinessEvents" "BusinessEvents" "BusinessEventDbContext"

# 2. Customers (core entity)
apply_migration "ECommerce.Modules.Customers" "Customers" "CustomerDbContext"

# 3. Products (core entity)
apply_migration "ECommerce.Modules.Products" "Products" "ProductDbContext"

# 4. Orders (depends on customers and products)
apply_migration "ECommerce.Modules.Orders" "Orders" "OrderDbContext"

echo ""
echo "🎉 All migrations applied successfully!"
echo ""
echo "📋 Summary:"
echo "   ✅ BusinessEvents module"
echo "   ✅ Customers module"
echo "   ✅ Products module"
echo "   ✅ Orders module"
echo ""
echo "🔗 Your PostgreSQL database is now ready for the Modular Monolith application."
