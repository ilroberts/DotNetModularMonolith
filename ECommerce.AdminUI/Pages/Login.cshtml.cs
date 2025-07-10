using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using ECommerce.AdminUI.Services;

namespace ECommerce.AdminUI.Pages
{
    public class LoginModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(IAuthService authService, ILogger<LoginModel> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [BindProperty]
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public IActionResult OnGet()
        {
            _logger.LogDebug("OnGet: Login page accessed. PathBase: {PathBase}, Path: {Path}, HasToken: {HasToken}",
                Request.PathBase, Request.Path, !string.IsNullOrEmpty(HttpContext.Session.GetString("AuthToken")));

            // If already logged in, redirect to dashboard
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("AuthToken")))
            {
                _logger.LogDebug("User has token, redirecting to Index");
                // Use RedirectToPage instead of Response.Redirect for more reliable path handling
                return RedirectToPage("Index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            _logger.LogDebug("OnPostAsync: Login form submitted. Username: {Username}", Username);

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // temporary hardcoded validation for demonstration purposes
            if (Password != "password123")
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return Page();
            }

            try
            {
                // Call the token generation API
                var token = await _authService.GenerateTokenAsync(Username);
                _logger.LogDebug("Token generation result: {TokenResult}", string.IsNullOrEmpty(token) ? "Failed" : "Success");

                if (string.IsNullOrEmpty(token))
                {
                    ModelState.AddModelError(string.Empty, "Failed to generate token. Please try again.");
                    return Page();
                }

                // Store token in session
                HttpContext.Session.SetString("AuthToken", token);
                HttpContext.Session.SetString("Username", Username);

                _logger.LogInformation("User {Username} logged in successfully", Username);

                // Redirect to dashboard - use relative path without leading slash
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user {Username}", Username);
                ModelState.AddModelError(string.Empty, "Login failed. Please try again.");
                return Page();
            }
        }
    }
}
