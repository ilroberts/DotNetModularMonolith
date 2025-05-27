using ECommerce.BusinessEvents.Infrastructure.Validators;
using ECommerce.BusinessEvents.Persistence;
using ECommerce.BusinessEvents.Services;
using ECommerce.Contracts.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.BusinessEvents
{
    public static class BusinessEventsModule
    {
        public static IServiceCollection AddBusinessEventsModule(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<BusinessEventDbContext>(options =>
            {
                options.UseInMemoryDatabase("ECommerce.BusinessEvents");
            });

            // Register SchemaRegistryService
            services.AddScoped<SchemaRegistryService>();
            services.AddScoped<SchemaInitializerService>();

            services.AddScoped<IBusinessEventService, EventTrackingService>();
            services.AddScoped<IJsonSchemaValidator, JsonSchemaValidator>();
            services.AddScoped<IEventRetrievalService>(sp =>
                (IEventRetrievalService)sp.GetRequiredService<IBusinessEventService>());

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

        public static async Task InitializeDefaultSchemasAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var schemaInitializer = scope.ServiceProvider.GetRequiredService<SchemaInitializerService>();
            await schemaInitializer.InitializeDefaultSchemasAsync();
        }
    }
}
