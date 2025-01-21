
using ECommerce.Modules.Customers.Persistence;
using ECommerce.Modules.Customers.Util;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Modules.Customers.Services;

internal class SuspensionTypeService(IServiceProvider serviceProvider)
{
    public async Task LoadSuspensionTypesAsync()
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CustomerDbContext>();
        var suspensionTypes = await dbContext.SuspensionTypes.ToListAsync();
        SuspensionTypeCode.LoadFromDatabase(suspensionTypes);
    }
}
