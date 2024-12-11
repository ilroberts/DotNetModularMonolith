// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using ECommerce.Modules.Customers.Domain;
using ECommerce.Modules.Customers.Persistence;
using ECommerce.Modules.Customers.Util;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ECommerce.Modules.Customers.Services;

public class DatabaseSeedingHostedService(IServiceProvider serviceProvider,
    ILogger<DatabaseSeedingHostedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CustomerDbContext>();

        if (!await dbContext.SuspensionTypes.AnyAsync())
        {
            logger.LogInformation("seeding database");
            SuspensionTypeSeedData.Seed(dbContext);
        }

        var suspensionTypeService = scope.ServiceProvider.GetRequiredService<SuspensionTypeService>();
        await suspensionTypeService.LoadSuspensionTypesAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
