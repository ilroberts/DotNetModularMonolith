using Microsoft.Playwright;
using ECommerce.UI.Tests.Helpers;

namespace ECommerce.UI.Tests.Pages;

/// <summary>
/// Page object representing the product creation page
/// </summary>
public class CreateProductPage(IPage page)
{
    // Element selectors
    private readonly string _pageTitle = "h1:has-text('Create New Product')";
    private readonly string _nameInput = "input#Product_Name";
    private readonly string _priceInput = "input#Product_Price";
    private readonly string _submitButton = "button[type='submit']";
    private readonly string _cancelButton = "a:has-text('Cancel')";
    private readonly string _validationErrors = ".text-danger";

    public async Task<bool> IsDisplayed()
    {
        return await page.ElementExistsAsync(_pageTitle);
    }

    public async Task FillProductForm(string name, decimal price)
    {
        await page.FillAsync(_nameInput, name);
        await page.FillAsync(_priceInput, price.ToString());
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

    public async Task<bool> CreateProduct(string name, decimal price)
    {
        await FillProductForm(name, price);
        await ClickSave();

        // Check if we're redirected to the product list (success) or still on the form (error)
        await page.WaitForNetworkIdleAsync();
        return !await IsDisplayed();
    }
}
