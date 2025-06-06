using Microsoft.Playwright;
using ECommerce.UI.Tests.Helpers;

namespace ECommerce.UI.Tests.Pages;

/// <summary>
/// Page object representing the customer creation page
/// </summary>
public class CreateCustomerPage(IPage page)
{
    // Element selectors
    private readonly string _pageTitle = "h1:has-text('Create Customer')";
    private readonly string _nameInput = "#Customer_Name";
    private readonly string _emailInput = "#Customer_Email";
    private readonly string _submitButton = "input[type='submit']"; // Updated to match the actual form element
    private readonly string _cancelButton = "a:has-text('Back to List')"; // Updated to match the actual text in the button
    private readonly string _validationErrors = ".text-danger";

    public async Task<bool> IsDisplayed()
    {
        return await page.ElementExistsAsync(_pageTitle);
    }

    public async Task FillCustomerForm(string name, string email, string? phone = null)
    {
        await page.FillAsync(_nameInput, name);
        await page.FillAsync(_emailInput, email);
    }

    public async Task ClickSave()
    {
        await page.ClickAsync(_submitButton);
        await page.WaitForNetworkIdleAsync();
    }

    public async Task ClickCancel()
    {
        await page.ClickAsync(_cancelButton);
        await page.WaitForNetworkIdleAsync();
    }

    public async Task<bool> HasValidationErrors()
    {
        return await page.ElementExistsAsync(_validationErrors);
    }

    public async Task<string?> GetValidationErrorMessage()
    {
        if (await page.ElementExistsAsync(_validationErrors))
        {
            return await page.TextContentAsync(_validationErrors);
        }
        return string.Empty;
    }

    public async Task<bool> CreateCustomer(string name, string email, string? phone = null)
    {
        await FillCustomerForm(name, email, phone);
        await ClickSave();

        // Check if we're redirected to the customer list (success) or still on the form (error)
        await page.WaitForNetworkIdleAsync();
        return !await IsDisplayed();
    }
}
