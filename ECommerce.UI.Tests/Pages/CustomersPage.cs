using Microsoft.Playwright;
using ECommerce.UI.Tests.Helpers;

namespace ECommerce.UI.Tests.Pages;

/// <summary>
/// Page object representing the customers page
/// </summary>
public class CustomersPage
{
    private readonly IPage _page;

    // Element selectors
    private readonly string _customersTitle = "h1:has-text('Customers')";
    private readonly string _addNewButton = "a:has-text('Add New Customer')"; // Updated to match actual text in button
    private readonly string _customersTable = "table.table";
    private readonly string _customerRows = "table.table tbody tr";
    private readonly string _successMessage = ".alert-success";

    public CustomersPage(IPage page)
    {
        _page = page;
    }

    public async Task NavigateAsync(string baseUrl)
    {
        await _page.GotoAsync($"{baseUrl}/Customers/Index");
        await _page.WaitForPageLoadAsync();
    }

    public async Task<bool> IsDisplayed()
    {
        return await _page.ElementExistsAsync(_customersTitle);
    }

    public async Task<int> GetCustomerCount()
    {
        if (!await _page.ElementExistsAsync(_customerRows))
        {
            return 0;
        }

        return await _page.Locator(_customerRows).CountAsync();
    }

    public async Task ClickAddNewCustomer()
    {
        await _page.ClickAsync(_addNewButton);
        await _page.WaitForNetworkIdleAsync();
    }

    public async Task<bool> CustomerExists(string customerName)
    {
        return await _page.ElementExistsAsync($"table.table td:has-text('{customerName}')");
    }

    public async Task ClickEditCustomer(string customerName)
    {
        var row = _page.Locator($"table.table tr:has-text('{customerName}')");
        await row.Locator(".btn-warning").ClickAsync();
        await _page.WaitForNetworkIdleAsync();
    }

    public async Task ClickDeleteCustomer(string customerName)
    {
        var row = _page.Locator($"table.table tr:has-text('{customerName}')");
        await row.Locator(".btn-danger").ClickAsync();
        await _page.WaitForNetworkIdleAsync();
    }

    public async Task ClickViewCustomerDetails(string customerName)
    {
        var row = _page.Locator($"table.table tr:has-text('{customerName}')");
        await row.Locator(".btn-info").ClickAsync();
        await _page.WaitForNetworkIdleAsync();
    }

    public async Task<string?> GetSuccessMessage()
    {
        if (await _page.ElementExistsAsync(_successMessage))
        {
            return await _page.TextContentAsync(_successMessage);
        }
        return string.Empty;
    }
}
