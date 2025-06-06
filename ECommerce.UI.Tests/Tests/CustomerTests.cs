using Microsoft.Playwright;
using ECommerce.UI.Tests.Pages;
using ECommerce.UI.Tests.Config;
using ECommerce.UI.Tests.Helpers;
using Microsoft.Extensions.Logging;

namespace ECommerce.UI.Tests.Tests;

[TestFixture]
public class CustomerTests : BaseTest
{
    private LoginPage _loginPage;
    private DashboardPage _dashboardPage;
    private CustomersPage _customersPage;
    private CreateCustomerPage _createCustomerPage;
    private ILogger<CustomerTests> _logger;

    [SetUp]
    public void SetUp()
    {
        // Create a simple console logger for test debugging
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        _logger = loggerFactory.CreateLogger<CustomerTests>();

        // Initialize page objects
        _loginPage = new LoginPage(Page);
        _dashboardPage = new DashboardPage(Page);
        _customersPage = new CustomersPage(Page);
        _createCustomerPage = new CreateCustomerPage(Page);
    }

    [Test]
    public async Task Should_CreateNewCustomer_When_ValidDataProvided()
    {
        try
        {
            _logger.LogInformation("Starting customer creation test");

            // Arrange
            // First login
            _logger.LogInformation("Navigating to the login page");
            await _loginPage.NavigateAsync(TestSettings.BaseUrl);
            _logger.LogInformation("Attempting to log in");
            await _loginPage.Login(TestSettings.DefaultUsername);
            _logger.LogInformation("Login completed");

            // Navigate to Customers page
            _logger.LogInformation("Navigating to Customers page");
            await _dashboardPage.NavigateToCustomers();
            if (!await _customersPage.IsDisplayed())
            {
                _logger.LogError("Failed to navigate to Customers page");
                await TakeDebugScreenshot("customers-page-navigation-failed");
            }
            Assert.That(await _customersPage.IsDisplayed(), Is.True, "Failed to navigate to Customers page");
            _logger.LogInformation("Successfully navigated to Customers page");

            // Generate unique customer name and email using timestamp
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            string customerName = $"Test Customer {timestamp}";
            string customerEmail = $"test.customer.{timestamp}@example.com";
            string customerPhone = "123-456-7890";
            _logger.LogInformation("Test data generated: Name={CustomerName}, Email={CustomerEmail}", customerName, customerEmail);

            // Act - Create new customer
            _logger.LogInformation("Clicking Add New Customer button");
            await _customersPage.ClickAddNewCustomer();
            if (!await _createCustomerPage.IsDisplayed())
            {
                _logger.LogError("Create customer page not displayed after clicking Add New");
                await TakeDebugScreenshot("create-customer-page-not-displayed");
            }
            Assert.That(await _createCustomerPage.IsDisplayed(), Is.True, "Create customer page not displayed");
            _logger.LogInformation("Successfully navigated to Create Customer page");

            _logger.LogInformation("Filling customer form and submitting");
            bool result = await _createCustomerPage.CreateCustomer(customerName, customerEmail, customerPhone);
            _logger.LogInformation("Form submission completed, result: {Result}", result);

            // Assert - Verify customer was created
            if (!result)
            {
                _logger.LogError("Failed to create customer - form submission failed");
                await TakeDebugScreenshot("customer-creation-failed");
            }
            Assert.That(result, Is.True, "Failed to create customer");

            if (!await _customersPage.IsDisplayed())
            {
                _logger.LogError("Not redirected to customers page after creation");
                await TakeDebugScreenshot("not-redirected-after-creation");
            }
            Assert.That(await _customersPage.IsDisplayed(), Is.True, "Not redirected to customers page after creation");
            _logger.LogInformation("Successfully redirected to Customers list page");

            if (!await _customersPage.CustomerExists(customerName))
            {
                _logger.LogError("Customer '{CustomerName}' not found in the list after creation", customerName);
                await TakeDebugScreenshot("customer-not-in-list");
            }
            Assert.That(await _customersPage.CustomerExists(customerName), Is.True, $"Customer '{customerName}' not found in the list");
            _logger.LogInformation("Customer successfully found in the list");

            // Verify success message
            string successMessage = await _customersPage.GetSuccessMessage();
            _logger.LogInformation("Success message found: {Message}", successMessage);
            Assert.That(successMessage, Contains.Substring("created successfully"), "Success message not displayed");

            _logger.LogInformation("Customer creation test completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Customer creation test failed with exception");
            await TakeDebugScreenshot("unexpected-error");
            throw;
        }
    }

    [Test]
    public async Task Should_ShowValidationError_When_CustomerEmailIsInvalid()
    {
        // Arrange
        // First login
        await _loginPage.NavigateAsync(TestSettings.BaseUrl);
        await _loginPage.Login(TestSettings.DefaultUsername);

        // Navigate to Customers page
        await _dashboardPage.NavigateToCustomers();

        // Generate test data with invalid email
        string customerName = $"Test Customer {DateTime.Now:yyyyMMddHHmmss}";
        string invalidEmail = "not-a-valid-email";

        // Act - Try to create customer with invalid email
        await _customersPage.ClickAddNewCustomer();
        await _createCustomerPage.FillCustomerForm(customerName, invalidEmail);
        await _createCustomerPage.ClickSave();

        // Assert
        Assert.That(await _createCustomerPage.IsDisplayed(), Is.True, "Should remain on create customer page");
        Assert.That(await _createCustomerPage.HasValidationErrors(), Is.True, "Validation errors should be displayed");
    }

    [Test]
    public async Task Should_ShowValidationError_When_CustomerNameIsEmpty()
    {
        // Arrange
        // First login
        await _loginPage.NavigateAsync(TestSettings.BaseUrl);
        await _loginPage.Login(TestSettings.DefaultUsername);

        // Navigate to Customers page
        await _dashboardPage.NavigateToCustomers();

        // Generate test data with empty name
        string emptyName = "";
        string customerEmail = $"test.customer.{DateTime.Now:yyyyMMddHHmmss}@example.com";

        // Act - Try to create customer with empty name
        await _customersPage.ClickAddNewCustomer();
        await _createCustomerPage.FillCustomerForm(emptyName, customerEmail);
        await _createCustomerPage.ClickSave();

        // Assert
        Assert.That(await _createCustomerPage.IsDisplayed(), Is.True, "Should remain on create customer page");
        Assert.That(await _createCustomerPage.HasValidationErrors(), Is.True, "Validation errors should be displayed");
    }

    // Helper method for taking debug screenshots
    private async Task TakeDebugScreenshot(string screenshotName)
    {
        try
        {
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            string fileName = $"{screenshotName}-{timestamp}.png";
            string filePath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "test-results", fileName);

            // Ensure directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            await Page.ScreenshotAsync(new() { Path = filePath, FullPage = true });
            _logger.LogInformation("Debug screenshot saved to: {FilePath}", filePath);
            TestContext.AddTestAttachment(filePath, $"Debug Screenshot: {screenshotName}");

            // Also capture page HTML for further debugging
            string htmlFileName = $"{screenshotName}-{timestamp}.html";
            string htmlFilePath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "test-results", htmlFileName);
            await File.WriteAllTextAsync(htmlFilePath, await Page.ContentAsync());
            _logger.LogInformation("Debug HTML saved to: {FilePath}", htmlFilePath);
            TestContext.AddTestAttachment(htmlFilePath, $"Debug HTML: {screenshotName}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to take debug screenshot");
        }
    }
}
