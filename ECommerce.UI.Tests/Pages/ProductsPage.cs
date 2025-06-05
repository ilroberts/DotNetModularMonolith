using Microsoft.Playwright;
using ECommerce.UI.Tests.Helpers;

namespace ECommerce.UI.Tests.Pages;

/// <summary>
/// Page object representing the products page
/// </summary>
public class ProductsPage
{
    private readonly IPage _page;

    // Element selectors
    private readonly string _productsTitle = "h1:has-text('Products')";
    private readonly string _addNewButton = "a:has-text('Add New Product')";
    private readonly string _productsTable = "table.table";
    private readonly string _productRows = "table.table tbody tr";
    private readonly string _successMessage = ".alert-success";

    public ProductsPage(IPage page)
    {
        _page = page;
    }

    public async Task NavigateAsync(string baseUrl)
    {
        await _page.GotoAsync($"{baseUrl}/Products/Index");
        await _page.WaitForPageLoadAsync();
    }

    public async Task<bool> IsDisplayed()
    {
        return await _page.ElementExistsAsync(_productsTitle);
    }

    public async Task<int> GetProductCount()
    {
        if (!await _page.ElementExistsAsync(_productRows))
        {
            return 0;
        }

        return await _page.Locator(_productRows).CountAsync();
    }

    public async Task ClickAddNewProduct()
    {
        await _page.ClickAsync(_addNewButton);
        await _page.WaitForNetworkIdleAsync();
    }

    public async Task<bool> ProductExists(string productName)
    {
        return await _page.ElementExistsAsync($"table.table td:has-text('{productName}')");
    }

    public async Task ClickEditProduct(string productName)
    {
        var row = _page.Locator($"table.table tr:has-text('{productName}')");
        await row.Locator(".btn-warning").ClickAsync();
        await _page.WaitForNetworkIdleAsync();
    }

    public async Task ClickDeleteProduct(string productName)
    {
        var row = _page.Locator($"table.table tr:has-text('{productName}')");
        await row.Locator(".btn-danger").ClickAsync();
        await _page.WaitForNetworkIdleAsync();
    }

    public async Task ClickViewProductDetails(string productName)
    {
        var row = _page.Locator($"table.table tr:has-text('{productName}')");
        await row.Locator(".btn-info").ClickAsync();
        await _page.WaitForNetworkIdleAsync();
    }

    public async Task<string> GetSuccessMessage()
    {
        if (await _page.ElementExistsAsync(_successMessage))
        {
            return await _page.TextContentAsync(_successMessage);
        }
        return string.Empty;
    }
}
