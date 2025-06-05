namespace ECommerce.UI.Tests.Config;

public static class TestSettings
{
    // Base URL for the application under test - updated to match actual URL
    public static string BaseUrl => Environment.GetEnvironmentVariable("TEST_BASE_URL") ?? "http://localhost:58000";

    // Browser configurations
    public static string DefaultBrowser => Environment.GetEnvironmentVariable("TEST_BROWSER") ?? "chromium";
    public static bool Headless => string.Equals(Environment.GetEnvironmentVariable("TEST_HEADLESS"), "false", StringComparison.OrdinalIgnoreCase);

    // Timeouts
    public static int DefaultTimeout => 30000; // 30 seconds
    public static int NavigationTimeout => 60000; // 60 seconds

    // Default test user credentials
    public static string DefaultUsername => Environment.GetEnvironmentVariable("TEST_USERNAME") ?? "admin";
}
