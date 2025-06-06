using Microsoft.Playwright;
using ECommerce.UI.Tests.Helpers;

namespace ECommerce.UI.Tests.Pages;

/// <summary>
/// Page object representing the customer edit page
/// </summary>
public class EditCustomerPage
{
    private readonly IPage _page;

    // Element selectors
    private readonly string _pageTitle = "h1:has-text('Edit Customer')";
    private readonly string _nameInput = "#Customer_Name";
    private readonly string _emailInput = "#Customer_Email";
    private readonly string _phoneInput = "#Customer_Phone";
    private readonly string _submitButton = "input[type='submit']";
    private readonly string _backToListButton = "a:has-text('Back to List')";
    private readonly string _validationErrors = ".text-danger";

    public EditCustomerPage(IPage page)
    {
        _page = page;
    }

    public async Task<bool> IsDisplayed()
    {
        return await _page.ElementExistsAsync(_pageTitle);
    }

    public async Task<string> GetCurrentName()
    {
        return await _page.InputValueAsync(_nameInput);
    }

    public async Task<string> GetCurrentEmail()
    {
        return await _page.InputValueAsync(_emailInput);
    }

    public async Task FillCustomerForm(string name, string email, string phone = null)
    {
        // Clear existing values first
        await _page.FillAsync(_nameInput, "");
        await _page.FillAsync(_emailInput, "");

        // Fill with new values
        await _page.FillAsync(_nameInput, name);
        await _page.FillAsync(_emailInput, email);

        if (!string.IsNullOrEmpty(phone) && await _page.ElementExistsAsync(_phoneInput))
        {
            await _page.FillAsync(_phoneInput, phone);
        }
    }

    public async Task ClickSave()
    {
        await _page.ClickAsync(_submitButton);
        await _page.WaitForNetworkIdleAsync();
    }

    public async Task ClickBackToList()
    {
        await _page.ClickAsync(_backToListButton);
        await _page.WaitForNetworkIdleAsync();
    }

    public async Task<bool> HasValidationErrors()
    {
        return await _page.ElementExistsAsync(_validationErrors);
    }

    public async Task<string> GetValidationErrorMessage()
    {
        if (await _page.ElementExistsAsync(_validationErrors))
        {
            return await _page.TextContentAsync(_validationErrors);
        }
        return string.Empty;
    }

    public async Task<bool> UpdateCustomer(string name, string email, string phone = null)
    {
        await FillCustomerForm(name, email, phone);
        await ClickSave();

        // Check if we're redirected to the customer list (success) or still on the form (error)
        await _page.WaitForNetworkIdleAsync();
        return !await IsDisplayed();
    }
}
