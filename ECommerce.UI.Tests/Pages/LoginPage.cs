using Microsoft.Playwright;
using ECommerce.UI.Tests.Helpers;

namespace ECommerce.UI.Tests.Pages;

/// <summary>
/// Page object representing the login page
/// </summary>
public class LoginPage
{
    private readonly IPage _page;

    // Element selectors
    private readonly string _usernameInput = "#Username";
    private readonly string _loginButton = "button[type='submit']";
    private readonly string _errorMessage = ".alert-danger";

    public LoginPage(IPage page)
    {
        _page = page;
    }

    public async Task NavigateAsync(string baseUrl)
    {
        await _page.GotoAsync($"{baseUrl}/login");
        await _page.WaitForPageLoadAsync();
    }

    public async Task<bool> IsDisplayed()
    {
        return await _page.ElementExistsAsync(_loginButton);
    }

    public async Task<bool> Login(string username)
    {
        await _page.FillAsync(_usernameInput, username);
        await _page.ClickAsync(_loginButton);

        // Check if login was successful (error message not present)
        await _page.WaitForNetworkIdleAsync();
        return !await _page.ElementExistsAsync(_errorMessage);
    }

    public async Task<string> GetErrorMessage()
    {
        if (await _page.ElementExistsAsync(_errorMessage))
        {
            return await _page.TextContentAsync(_errorMessage);
        }
        return string.Empty;
    }
}
