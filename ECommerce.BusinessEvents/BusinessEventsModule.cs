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

            // Register EventTrackingService and its interface
            services.AddScoped<IEventTrackingService, EventTrackingService>();
            // Register BusinessEventService as the IBusinessEventService for other modules
            services.AddScoped<IBusinessEventService, BusinessEventService>();
            services.AddScoped<IJsonSchemaValidator, JsonSchemaValidator>();

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
