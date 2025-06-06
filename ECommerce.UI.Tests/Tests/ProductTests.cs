using ECommerce.UI.Tests.Pages;
using ECommerce.UI.Tests.Config;
using Microsoft.Extensions.Logging;

namespace ECommerce.UI.Tests.Tests;

[TestFixture]
public class ProductTests : BaseTest
{
    private LoginPage _loginPage;
    private DashboardPage _dashboardPage;
    private ProductsPage _productsPage;
    private CreateProductPage _createProductPage;
    private EditProductPage _editProductPage;
    private ILogger<ProductTests> _logger;

    [SetUp]
    public void SetUp()
    {
        // Create a simple console logger for test debugging
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        _logger = loggerFactory.CreateLogger<ProductTests>();

        // Initialize page objects
        _loginPage = new LoginPage(Page);
        _dashboardPage = new DashboardPage(Page);
        _productsPage = new ProductsPage(Page);
        _createProductPage = new CreateProductPage(Page);
        _editProductPage = new EditProductPage(Page);
    }

    [Test]
    public async Task Should_CreateNewProduct_When_ValidDataProvided()
    {
        try
        {
            _logger.LogInformation("Starting product creation test");

            // Arrange
            // First login
            _logger.LogInformation("Navigating to the login page");
            await _loginPage.NavigateAsync(TestSettings.BaseUrl);
            _logger.LogInformation("Attempting to log in");
            await _loginPage.Login(TestSettings.DefaultUsername);
            _logger.LogInformation("Login completed");

            // Navigate to Products page
            _logger.LogInformation("Navigating to Products page");
            await _dashboardPage.NavigateToProducts();
            if (!await _productsPage.IsDisplayed())
            {
                _logger.LogError("Failed to navigate to Products page");
                await TakeDebugScreenshot("products-page-navigation-failed");
            }
            Assert.That(await _productsPage.IsDisplayed(), Is.True, "Failed to navigate to Products page");
            _logger.LogInformation("Successfully navigated to Products page");

            // Generate unique product name using timestamp
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            string productName = $"Test Product {timestamp}";
            decimal productPrice = 99.99m;
            _logger.LogInformation("Test data generated: Name={ProductName}, Price={ProductPrice}", productName, productPrice);

            // Act - Create new product
            _logger.LogInformation("Clicking Add New Product button");
            await _productsPage.ClickAddNewProduct();
            if (!await _createProductPage.IsDisplayed())
            {
                _logger.LogError("Create product page not displayed after clicking Add New");
                await TakeDebugScreenshot("create-product-page-not-displayed");
            }
            Assert.That(await _createProductPage.IsDisplayed(), Is.True, "Create product page not displayed");
            _logger.LogInformation("Successfully navigated to Create Product page");

            _logger.LogInformation("Filling product form and submitting");
            bool result = await _createProductPage.CreateProduct(productName, productPrice);
            _logger.LogInformation("Form submission completed, result: {Result}", result);

            // Assert - Verify product was created
            if (!result)
            {
                _logger.LogError("Failed to create product - form submission failed");
                await TakeDebugScreenshot("product-creation-failed");
            }
            Assert.That(result, Is.True, "Failed to create product");

            if (!await _productsPage.IsDisplayed())
            {
                _logger.LogError("Not redirected to products page after creation");
                await TakeDebugScreenshot("not-redirected-after-creation");
            }
            Assert.That(await _productsPage.IsDisplayed(), Is.True, "Not redirected to products page after creation");
            _logger.LogInformation("Successfully redirected to Products list page");

            if (!await _productsPage.ProductExists(productName))
            {
                _logger.LogError("Product '{ProductName}' not found in the list after creation", productName);
                await TakeDebugScreenshot("product-not-in-list");
            }
            Assert.That(await _productsPage.ProductExists(productName), Is.True, $"Product '{productName}' not found in the list");
            _logger.LogInformation("Product successfully found in the list");

            // Verify success message
            string? successMessage = await _productsPage.GetSuccessMessage();
            _logger.LogInformation("Success message found: {Message}", successMessage);
            Assert.That(successMessage, Contains.Substring("created successfully"), "Success message not displayed");

            _logger.LogInformation("Product creation test completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Product creation test failed with exception");
            await TakeDebugScreenshot("unexpected-error");
            throw;
        }
    }

    [Test]
    public async Task Should_UpdateProduct_When_ValidDataProvided()
    {
        try
        {
            _logger.LogInformation("Starting product update test");

            // Arrange - First create a product that we can then update
            _logger.LogInformation("Navigating to the login page");
            await _loginPage.NavigateAsync(TestSettings.BaseUrl);
            await _loginPage.Login(TestSettings.DefaultUsername);

            // Navigate to Products page
            _logger.LogInformation("Navigating to Products page");
            await _dashboardPage.NavigateToProducts();

            // Create a product to update
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            string originalName = $"Update Test Product {timestamp}";
            decimal originalPrice = 49.99m;

            _logger.LogInformation("Creating a test product to update: {Name}, {Price}", originalName, originalPrice);
            await _productsPage.ClickAddNewProduct();
            bool createResult = await _createProductPage.CreateProduct(originalName, originalPrice);

            if (!createResult || !await _productsPage.ProductExists(originalName))
            {
                _logger.LogError("Failed to create test product for update test");
                await TakeDebugScreenshot("create-test-product-failed");
                Assert.Fail("Could not create test product for update test");
                return;
            }

            // Act - Update the product we just created
            _logger.LogInformation("Finding and editing the test product");
            await _productsPage.ClickEditProduct(originalName);

            if (!await _editProductPage.IsDisplayed())
            {
                _logger.LogError("Edit product page not displayed");
                await TakeDebugScreenshot("edit-page-not-displayed");
                Assert.Fail("Edit product page did not load");
                return;
            }

            // Verify current values
            string currentName = await _editProductPage.GetCurrentName();
            decimal currentPrice = await _editProductPage.GetCurrentPrice();

            _logger.LogInformation("Current values - Name: {Name}, Price: {Price}", currentName, currentPrice);
            Assert.That(currentName, Is.EqualTo(originalName), "Original name not displayed in edit form");
            Assert.That(currentPrice, Is.EqualTo(originalPrice), "Original price not displayed in edit form");

            // Update with new values
            string updatedName = $"Updated {originalName}";
            decimal updatedPrice = 79.99m;

            _logger.LogInformation("Updating product with new values - Name: {Name}, Price: {Price}", updatedName, updatedPrice);
            bool updateResult = await _editProductPage.UpdateProduct(updatedName, updatedPrice);

            // Assert - Verify product was updated
            if (!updateResult)
            {
                _logger.LogError("Failed to update product - form submission failed");
                await TakeDebugScreenshot("product-update-failed");
            }
            Assert.That(updateResult, Is.True, "Failed to update product");

            if (!await _productsPage.IsDisplayed())
            {
                _logger.LogError("Not redirected to products page after update");
                await TakeDebugScreenshot("not-redirected-after-update");
            }
            Assert.That(await _productsPage.IsDisplayed(), Is.True, "Not redirected to products page after update");

            // Check that product with new name exists in list
            if (!await _productsPage.ProductExists(updatedName))
            {
                _logger.LogError("Updated product '{ProductName}' not found in the list", updatedName);
                await TakeDebugScreenshot("updated-product-not-in-list");
            }
            Assert.That(await _productsPage.ProductExists(updatedName), Is.True, $"Updated product '{updatedName}' not found in the list");

            // Verify success message
            string? successMessage = await _productsPage.GetSuccessMessage();
            _logger.LogInformation("Success message found: {Message}", successMessage);
            Assert.That(successMessage, Contains.Substring("updated successfully"), "Success message not displayed");

            _logger.LogInformation("Product update test completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Product update test failed with exception");
            await TakeDebugScreenshot("update-unexpected-error");
            throw;
        }
    }

    [Test]
    public async Task Should_ShowValidationError_When_ProductNameIsEmpty()
    {
        // Arrange
        // First login
        await _loginPage.NavigateAsync(TestSettings.BaseUrl);
        await _loginPage.Login(TestSettings.DefaultUsername);

        // Navigate to Products page
        await _dashboardPage.NavigateToProducts();

        // Generate test data with empty name
        string emptyName = "";
        decimal productPrice = 10.00m;

        // Act - Try to create product with empty name
        await _productsPage.ClickAddNewProduct();
        await _createProductPage.FillProductForm(emptyName, productPrice);
        await _createProductPage.ClickSave();

        // Assert
        Assert.That(await _createProductPage.IsDisplayed(), Is.True, "Should remain on create product page");
        Assert.That(await _createProductPage.HasValidationErrors(), Is.True, "Validation errors should be displayed");
    }

    [Test]
    public async Task Should_ShowValidationError_When_PriceIsInvalid()
    {
        // Arrange
        // First login
        await _loginPage.NavigateAsync(TestSettings.BaseUrl);
        await _loginPage.Login(TestSettings.DefaultUsername);

        // Navigate to Products page
        await _dashboardPage.NavigateToProducts();

        // Generate test data with invalid price
        string productName = $"Test Product {DateTime.Now:yyyyMMddHHmmss}";
        decimal invalidPrice = -10.00m;

        // Act - Try to create product with invalid price
        await _productsPage.ClickAddNewProduct();
        await _createProductPage.FillProductForm(productName, invalidPrice);
        await _createProductPage.ClickSave();

        // Assert
        Assert.That(await _createProductPage.IsDisplayed(), Is.True, "Should remain on create product page");
        Assert.That(await _createProductPage.HasValidationErrors(), Is.True, "Validation errors should be displayed");
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
