using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using ECommerce.AdminUI.Services;

namespace ECommerce.AdminUI.Pages
{
    public class LoginModel : PageModel
    {
        private readonly AuthService _authService;
        private readonly ILogger<LoginModel> _logger;
        
        public LoginModel(AuthService authService, ILogger<LoginModel> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [BindProperty]
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; } = string.Empty;

        public void OnGet()
        {
            // If already logged in, redirect to dashboard
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("AuthToken")))
            {
                Response.Redirect("/Index");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // Call the token generation API
                var token = await _authService.GenerateTokenAsync(Username);
                
                if (string.IsNullOrEmpty(token))
                {
                    ModelState.AddModelError(string.Empty, "Failed to generate token. Please try again.");
                    return Page();
                }
                
                // Store token in session
                HttpContext.Session.SetString("AuthToken", token);
                HttpContext.Session.SetString("Username", Username);
                
                _logger.LogInformation("User {Username} logged in successfully", Username);
                
                // Redirect to dashboard
                return RedirectToPage("/Index");
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
