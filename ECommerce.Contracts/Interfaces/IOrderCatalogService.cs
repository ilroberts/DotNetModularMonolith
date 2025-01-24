// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using ECommerce.Contracts.DTOs;

namespace ECommerce.Contracts.Interfaces;

public interface IOrderCatalogService
{
    Task<OrderDto?> GetOrderByProductIdAsync(Guid productId);
}
