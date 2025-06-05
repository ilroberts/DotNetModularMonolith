using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using ECommerce.UI.Tests.Config;
using ECommerce.UI.Tests.Helpers;
using Microsoft.Extensions.Logging;

namespace ECommerce.UI.Tests;

[Parallelizable(ParallelScope.Self)]
public class BaseTest : PageTest
{
    protected virtual string StartUrl => TestSettings.BaseUrl;
    private readonly ILogger<BaseTest> _logger;

    public BaseTest()
    {
        // Create a simple console logger for test debugging
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        _logger = loggerFactory.CreateLogger<BaseTest>();
    }

    [SetUp]
    public async Task SetUpTest()
    {
        // Configure Playwright specific settings for each test
        await Context.Tracing.StartAsync(new()
        {
            Screenshots = true,
            Snapshots = true,
        });
    }

    [TearDown]
    public async Task TearDownTest()
    {
        // Capture trace on test failure
        if (TestContext.CurrentContext.Result.Outcome.Status == NUnit.Framework.Interfaces.TestStatus.Failed)
        {
            string traceFileName = $"trace-{TestContext.CurrentContext.Test.Name}-{DateTime.Now:yyyyMMdd_HHmmss}.zip";
            string tracePath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "test-results", traceFileName);

            // Ensure directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(tracePath));

            // Save trace for debugging
            await Context.Tracing.StopAsync(new() { Path = tracePath });

            // Attach screenshot if available
            try
            {
                string screenshotFileName = $"screenshot-{TestContext.CurrentContext.Test.Name}-{DateTime.Now:yyyyMMdd_HHmmss}.png";
                string screenshotPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "test-results", screenshotFileName);
                await Page.ScreenshotAsync(new() { Path = screenshotPath, FullPage = true });
                TestContext.AddTestAttachment(screenshotPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to capture screenshot: {ex.Message}");
            }
        }
        else
        {
            await Context.Tracing.StopAsync();
        }
    }

    public override BrowserNewContextOptions ContextOptions()
    {
        return new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 },
            RecordVideoDir = "test-results/videos/",
            IgnoreHTTPSErrors = true
        };
    }

    protected async Task Login(string? username = null, string? password = null)
    {
        username ??= TestSettings.DefaultUsername;

        _logger.LogInformation($"Navigating to {StartUrl}/login");
        await Page.GotoAsync($"{StartUrl}/login");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        _logger.LogInformation("Filling username field");
        // Let's use multiple selector approaches to make it more robust
        try
        {
            // First try with for attribute
            await Page.Locator("label[for='Username'] + div input").FillAsync(username);
        }
        catch
        {
            try
            {
                // Try with id
                await Page.Locator("#Username").FillAsync(username);
            }
            catch
            {
                try
                {
                    // Try with name
                    await Page.Locator("input[name='Username']").FillAsync(username);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to find username field: {ex.Message}");

                    // Take screenshot for debugging
                    var screenshotPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "login-debug.png");
                    await Page.ScreenshotAsync(new() { Path = screenshotPath, FullPage = true });
                    TestContext.AddTestAttachment(screenshotPath);

                    // Get HTML content for debugging
                    var html = await Page.ContentAsync();
                    _logger.LogInformation($"Page HTML: {html}");

                    throw;
                }
            }
        }

        _logger.LogInformation("Clicking login button");
        await Page.ClickAsync("button[type='submit']");

        _logger.LogInformation("Waiting for navigation after login");
        await Page.WaitForURLAsync($"{StartUrl}/**");
    }
}
