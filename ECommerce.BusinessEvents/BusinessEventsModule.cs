using ECommerce.BusinessEvents.Persistence;
using ECommerce.BusinessEvents.Services;
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

            services.AddScoped<EventTrackingService>();

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
