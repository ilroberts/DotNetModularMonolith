using Microsoft.Playwright;
using ECommerce.UI.Tests.Helpers;

namespace ECommerce.UI.Tests.Pages;

/// <summary>
/// Page object representing the products page
/// </summary>
public class ProductsPage(IPage page)
{
    // Element selectors
    private readonly string _productsTitle = "h1:has-text('Products')";
    private readonly string _addNewButton = "a:has-text('Add New Product')";
    private readonly string _productRows = "table.table tbody tr";
    private readonly string _successMessage = ".alert-success";

    public async Task NavigateAsync(string baseUrl)
    {
        await page.GotoAsync($"{baseUrl}/Products/Index");
        await page.WaitForPageLoadAsync();
    }

    public async Task<bool> IsDisplayed()
    {
        return await page.ElementExistsAsync(_productsTitle);
    }

    public async Task<int> GetProductCount()
    {
        if (!await page.ElementExistsAsync(_productRows))
        {
            return 0;
        }

        return await page.Locator(_productRows).CountAsync();
    }

    public async Task ClickAddNewProduct()
    {
        await page.ClickAsync(_addNewButton);
        await page.WaitForNetworkIdleAsync();
    }

    public async Task<bool> ProductExists(string productName)
    {
        return await page.ElementExistsAsync($"table.table td:has-text('{productName}')");
    }

    public async Task ClickEditProduct(string productName)
    {
        var row = page.Locator($"table.table tr:has-text('{productName}')");
        await row.Locator(".btn-warning").ClickAsync();
        await page.WaitForNetworkIdleAsync();
    }

    public async Task ClickDeleteProduct(string productName)
    {
        var row = page.Locator($"table.table tr:has-text('{productName}')");
        await row.Locator(".btn-danger").ClickAsync();
        await page.WaitForNetworkIdleAsync();
    }

    public async Task ClickViewProductDetails(string productName)
    {
        var row = page.Locator($"table.table tr:has-text('{productName}')");
        await row.Locator(".btn-info").ClickAsync();
        await page.WaitForNetworkIdleAsync();
    }

    public async Task<string?> GetSuccessMessage()
    {
        if (await page.ElementExistsAsync(_successMessage))
        {
            return await page.TextContentAsync(_successMessage);
        }
        return string.Empty;
    }
}
