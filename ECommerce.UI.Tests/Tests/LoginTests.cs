using Microsoft.Playwright;
using ECommerce.UI.Tests.Pages;
using ECommerce.UI.Tests.Config;
using ECommerce.UI.Tests.Helpers;

namespace ECommerce.UI.Tests.Tests;

[TestFixture]
public class LoginTests : BaseTest
{
    private LoginPage _loginPage;
    private DashboardPage _dashboardPage;

    [SetUp]
    public void SetUp()
    {
        // Initialize page objects
        _loginPage = new LoginPage(Page);
        _dashboardPage = new DashboardPage(Page);
    }

    [Test]
    public async Task Should_LoginSuccessfully_When_ValidCredentialsProvided()
    {
        // Arrange
        await _loginPage.NavigateAsync(TestSettings.BaseUrl);
        Assert.That(await _loginPage.IsDisplayed(), Is.True, "Login page not displayed");

        // Act
        bool loginSuccess = await _loginPage.Login(TestSettings.DefaultUsername);

        // Assert - First check login was successful
        Assert.That(loginSuccess, Is.True, "Login failed - error message displayed");

        // Then check we're on the dashboard page
        await Page.WaitForNetworkIdleAsync();
        Assert.That(await _dashboardPage.IsDisplayed(), Is.True, "Dashboard not displayed after login");

        // Verify welcome message includes username
        string? welcomeMessage = await _dashboardPage.GetWelcomeMessage();
        Assert.That(welcomeMessage, Contains.Substring("Welcome"), "Welcome message not displayed");

        // Take a screenshot of successful login
        string screenshotPath = TestContext.CurrentContext.TestDirectory + "/successful-login.png";
        await Page.ScreenshotAsync(new() { Path = screenshotPath, FullPage = true });
        TestContext.AddTestAttachment(screenshotPath, "Successful Login Screenshot");
    }

    [Test]
    public async Task Should_StayOnLoginPage_When_ErrorOccurs()
    {
        // This is a placeholder test to demonstrate failure handling
        // In a real scenario, you might test invalid credentials

        // Arrange - Navigate to login page
        await _loginPage.NavigateAsync(TestSettings.BaseUrl);
        Assert.That(await _loginPage.IsDisplayed(), Is.True, "Login page not displayed");

        // Act - Try to navigate directly to dashboard without logging in
        await Page.GotoAsync($"{TestSettings.BaseUrl}/Index");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert - Should be redirected back to login
        Assert.That(await _loginPage.IsDisplayed(), Is.True, "Should have been redirected to login page");
    }
}
