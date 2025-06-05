using Microsoft.Playwright;

namespace ECommerce.UI.Tests.Helpers;

public static class PlaywrightHelpers
{
    /// <summary>
    /// Waits for a page to be in a ready state
    /// </summary>
    public static async Task WaitForPageLoadAsync(this IPage page, int timeout = 30000)
    {
        await page.WaitForFunctionAsync("() => document.readyState === 'complete'", new PageWaitForFunctionOptions { Timeout = timeout });
    }

    /// <summary>
    /// Waits for all network requests to complete
    /// </summary>
    public static async Task WaitForNetworkIdleAsync(this IPage page, int timeout = 30000)
    {
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new() { Timeout = timeout });
    }

    /// <summary>
    /// Waits for an element to be visible and clickable
    /// </summary>
    public static async Task WaitForElementToBeClickableAsync(this IPage page, string selector, int timeout = 30000)
    {
        await page.WaitForSelectorAsync(selector, new() { State = WaitForSelectorState.Visible, Timeout = timeout });
    }

    /// <summary>
    /// Helper method to select an option from a dropdown by its visible text
    /// </summary>
    public static async Task SelectByTextAsync(this IPage page, string selector, string text)
    {
        await page.EvaluateAsync($@"
            selector => {{
                const dropdown = document.querySelector(selector);
                if (dropdown) {{
                    for (const option of dropdown.options) {{
                        if (option.text === '{text}') {{
                            option.selected = true;
                            dropdown.dispatchEvent(new Event('change', {{ bubbles: true }}));
                            return true;
                        }}
                    }}
                }}
                return false;
            }}", selector);
    }

    /// <summary>
    /// Checks if an element exists on the page
    /// </summary>
    public static async Task<bool> ElementExistsAsync(this IPage page, string selector)
    {
        var element = await page.QuerySelectorAsync(selector);
        return element != null;
    }
}
