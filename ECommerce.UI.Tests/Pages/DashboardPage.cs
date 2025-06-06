using Microsoft.Playwright;
using ECommerce.UI.Tests.Helpers;

namespace ECommerce.UI.Tests.Pages;

/// <summary>
/// Page object representing the dashboard page
/// </summary>
public class DashboardPage(IPage page)
{
    // Element selectors
    private readonly string _dashboardTitle = "h1:has-text('Dashboard')";
    private readonly string _welcomeMessage = "p:has-text('Welcome')";
    private readonly string _statsCards = ".card";
    private readonly string _customersLink = "a:has-text('Customers')";
    private readonly string _productsLink = "a:has-text('Products')";
    private readonly string _ordersLink = "a:has-text('Orders')";

    public async Task<bool> IsDisplayed()
    {
        // Check for dashboard title and stats cards
        return await page.ElementExistsAsync(_dashboardTitle) &&
               await page.ElementExistsAsync(_statsCards);
    }

    public async Task<string?> GetWelcomeMessage()
    {
        if (await page.ElementExistsAsync(_welcomeMessage))
        {
            return await page.TextContentAsync(_welcomeMessage);
        }
        return string.Empty;
    }

    public async Task NavigateToCustomers()
    {
        await page.ClickAsync(_customersLink);
        await page.WaitForNetworkIdleAsync();
    }

    public async Task NavigateToProducts()
    {
        await page.ClickAsync(_productsLink);
        await page.WaitForNetworkIdleAsync();
    }

    public async Task NavigateToOrders()
    {
        await page.ClickAsync(_ordersLink);
        await page.WaitForNetworkIdleAsync();
    }
}
