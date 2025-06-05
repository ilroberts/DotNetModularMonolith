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
}
