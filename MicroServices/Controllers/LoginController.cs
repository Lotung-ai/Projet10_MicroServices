using MicroServices.Models;
using MicroServices.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MicroServices.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly SignInManager<User> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly ILogger<LoginController> _logger;

        public LoginController(SignInManager<User> signInManager, ITokenService tokenService, ILogger<LoginController> logger)
        {
            _signInManager = signInManager;
            _tokenService = tokenService;
            _logger = logger;
        }

        // POST api/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Login model)
        {


            // Attempt to sign in the user
            var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                var user = await _signInManager.UserManager.FindByNameAsync(model.UserName);
                var token = await _tokenService.GenerateTokenAsync(user);
                _logger.LogInformation("User {UserName} logged in successfully.", model.UserName);
                return Ok(new { Message = "Login successful.", Token = token });
            }


            _logger.LogWarning("Invalid login attempt for user {UserName}.", model.UserName);
            return Unauthorized(new { Message = "Invalid login attempt." });
        }

        // POST api/logout
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out successfully.");
            return Ok(new { Message = "Logout successful." });
        }
    }
}
