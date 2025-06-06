using Microsoft.Playwright;
using ECommerce.UI.Tests.Helpers;

namespace ECommerce.UI.Tests.Pages;

/// <summary>
/// Page object representing the product edit page
/// </summary>
public class EditProductPage(IPage page)
{
    // Element selectors
    private readonly string _pageTitle = "h1:has-text('Edit Product')";
    private readonly string _nameInput = "#Product_Name";
    private readonly string _priceInput = "#Product_Price";
    private readonly string _submitButton = "input[type='submit']";
    private readonly string _cancelButton = "a:has-text('Cancel')";
    private readonly string _validationErrors = ".text-danger";

    public async Task<bool> IsDisplayed()
    {
        return await page.ElementExistsAsync(_pageTitle);
    }

    public async Task<string> GetCurrentName()
    {
        return await page.InputValueAsync(_nameInput);
    }

    public async Task<decimal> GetCurrentPrice()
    {
        string priceText = await page.InputValueAsync(_priceInput);
        if (decimal.TryParse(priceText, out decimal price))
        {
            return price;
        }
        return 0;
    }

    public async Task FillProductForm(string name, decimal price)
    {
        // Clear existing values first
        await page.FillAsync(_nameInput, "");
        await page.FillAsync(_priceInput, "");

        // Fill with new values
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

    public async Task<bool> UpdateProduct(string name, decimal price)
    {
        await FillProductForm(name, price);
        await ClickSave();

        // Check if we're redirected to the product list (success) or still on the form (error)
        await page.WaitForNetworkIdleAsync();
        return !await IsDisplayed();
    }
}
