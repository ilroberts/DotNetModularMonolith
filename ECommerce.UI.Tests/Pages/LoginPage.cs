using Microsoft.Playwright;
using ECommerce.UI.Tests.Helpers;

namespace ECommerce.UI.Tests.Pages;

/// <summary>
/// Page object representing the login page
/// </summary>
public class LoginPage(IPage page)
{
    // Element selectors
    private readonly string _usernameInput = "#Username";
    private readonly string _loginButton = "button[type='submit']";
    private readonly string _errorMessage = ".alert-danger";

    public async Task NavigateAsync(string baseUrl)
    {
        await page.GotoAsync($"{baseUrl}/login");
        await page.WaitForPageLoadAsync();
    }

    public async Task<bool> IsDisplayed()
    {
        return await page.ElementExistsAsync(_loginButton);
    }

    public async Task<bool> Login(string username)
    {
        await page.FillAsync(_usernameInput, username);
        await page.ClickAsync(_loginButton);

        // Check if login was successful (error message not present)
        await page.WaitForNetworkIdleAsync();
        return !await page.ElementExistsAsync(_errorMessage);
    }

    public async Task<string?> GetErrorMessage()
    {
        if (await page.ElementExistsAsync(_errorMessage))
        {
            return await page.TextContentAsync(_errorMessage);
        }
        return string.Empty;
    }
}
