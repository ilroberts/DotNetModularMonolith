using ECommerce.BusinessEvents.Infrastructure.Validators;
using ECommerce.BusinessEvents.Persistence;
using ECommerce.BusinessEvents.Services;
using ECommerce.Contracts.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ECommerce.BusinessEvents
{
    public static class BusinessEventsModule
    {
        public static IServiceCollection AddBusinessEventsModule(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            var serviceProvider = services.BuildServiceProvider();
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            var logger = loggerFactory?.CreateLogger("BusinessEventsModule");

            services.AddDbContext<BusinessEventDbContext>(options =>
            {
                if (!string.IsNullOrEmpty(connectionString))
                {
                    logger?.LogInformation("BusinessEventsModule: Using PostgreSQL connection string: {ConnectionString}",
                        connectionString.Substring(0, Math.Min(50, connectionString.Length)) + "...");

                    options.UseNpgsql(connectionString, npgsqlOptions =>
                    {
                        npgsqlOptions.MigrationsAssembly("ECommerce.BusinessEvents");
                    });
                }
                else
                {
                    logger?.LogInformation("BusinessEventsModule: No connection string found, using in-memory database");
                    options.UseInMemoryDatabase("ECommerce.Business.Events");
                }
            });

            // Register SchemaRegistryService
            services.AddScoped<SchemaRegistryService>();
            services.AddScoped<IBusinessEventService, EventTrackingService>();
            services.AddScoped<IJsonSchemaValidator, JsonSchemaValidator>();
            services.AddScoped<IEventRetrievalService>(sp =>
                (IEventRetrievalService)sp.GetRequiredService<IBusinessEventService>());

            // Register new EventQueryService for metadata-based queries
            services.AddScoped<IEventQueryService, EventQueryService>();

            // Add other services as needed

            return services;
        }

        public static void SeedData(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<BusinessEventDbContext>();
            context.Database.EnsureCreated();
            // Add seed logic if needed
        }
    }
}
