// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using ECommerce.Contracts.DTOs;
using ECommerce.Contracts.Interfaces;

namespace ECommerce.Modules.Orders.Services;

public class OrderCatalogService : IOrderCatalogService
{
    public Task<OrderDto?> GetOrderByProductIdAsync(Guid productId)
    {
        var order = new OrderDto
        {
            Id = Guid.NewGuid()
        };
        return Task.FromResult<OrderDto?>(order ?? null);
    }
}
