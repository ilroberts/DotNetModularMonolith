using Microsoft.Playwright;
using ECommerce.UI.Tests.Helpers;

namespace ECommerce.UI.Tests.Pages;

/// <summary>
/// Page object representing the product creation page
/// </summary>
public class CreateProductPage
{
    private readonly IPage _page;

    // Element selectors
    private readonly string _pageTitle = "h1:has-text('Create New Product')";
    private readonly string _nameInput = "input#Product_Name";
    private readonly string _priceInput = "input#Product_Price";
    private readonly string _submitButton = "button[type='submit']";
    private readonly string _cancelButton = "a:has-text('Cancel')";
    private readonly string _validationErrors = ".text-danger";

    public CreateProductPage(IPage page)
    {
        _page = page;
    }

    public async Task<bool> IsDisplayed()
    {
        return await _page.ElementExistsAsync(_pageTitle);
    }

    public async Task FillProductForm(string name, decimal price)
    {
        await _page.FillAsync(_nameInput, name);
        await _page.FillAsync(_priceInput, price.ToString());
    }

    public async Task ClickSave()
    {
        await _page.ClickAsync(_submitButton);
        await _page.WaitForNetworkIdleAsync();
    }

    public async Task ClickCancel()
    {
        await _page.ClickAsync(_cancelButton);
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

    public async Task<bool> CreateProduct(string name, decimal price)
    {
        await FillProductForm(name, price);
        await ClickSave();

        // Check if we're redirected to the product list (success) or still on the form (error)
        await _page.WaitForNetworkIdleAsync();
        return !await IsDisplayed();
    }
}
