using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MicroServices.Models;
using MicroServices.Services.Interfaces;

namespace MicroServices.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userRepository;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userRepository, UserManager<User> userManager, RoleManager<IdentityRole<int>> roleManager, ILogger<UserController> logger)
        {
            _userRepository = userRepository;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] Register register)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid registration attempt: {Errors}", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return BadRequest(ModelState);
            }

            // Vérifier si le rôle existe, sinon le créer
            if (!await _roleManager.RoleExistsAsync(register.Role))
            {
                var createRoleResult = await _roleManager.CreateAsync(new IdentityRole<int> { Name = register.Role });
                if (!createRoleResult.Succeeded)
                {
                    _logger.LogError("Failed to create role: {Role}", register.Role);
                    return StatusCode(StatusCodes.Status500InternalServerError, "Unable to create role.");
                }
            }

            var user = new User
            {
                UserName = register.UserName,
                Email = register.Email,
                Fullname = register.Fullname,
                Role = register.Role
            };

            // Créer l'utilisateur avec un mot de passe
            var result = await _userManager.CreateAsync(user, register.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, register.Role);
                _logger.LogInformation("User registered successfully: {UserId}", user.Id);
                return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
            }

            var errors = result.Errors.Select(e => e.Description);
            _logger.LogWarning("User registration failed: {Errors}", string.Join(", ", errors));
            return BadRequest(new { Errors = errors });
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "User, Admin")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("User not found: {UserId}", id);
                return NotFound();
            }

            _logger.LogInformation("User retrieved successfully: {UserId}", id);
            return Ok(user);
        }

        [HttpGet]
        [Authorize(Roles = "User, Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userRepository.GetAllUsersAsync();
            _logger.LogInformation("Retrieved {UserCount} users", users.Count());
            return Ok(users);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "User, Admin")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] Register register)
        {
            if (id <= 0 || register == null)
            {
                _logger.LogWarning("Invalid update attempt: Id = {Id}, Register = {Register}", id, register);
                return BadRequest("Invalid ID or request body.");
            }

            try
            {
                var user = await _userManager.FindByIdAsync(id.ToString());
                if (user == null)
                {
                    _logger.LogWarning("User not found for update: {UserId}", id);
                    return NotFound();
                }

                user.UserName = register.UserName;
                user.Email = register.Email;
                user.Fullname = register.Fullname;

                // Gérer le changement de rôle
                if (user.Role != register.Role)
                {
                    await _userManager.RemoveFromRoleAsync(user, user.Role);
                    if (!await _roleManager.RoleExistsAsync(register.Role))
                    {
                        await _roleManager.CreateAsync(new IdentityRole<int> { Name = register.Role });
                    }
                    await _userManager.AddToRoleAsync(user, register.Role);
                    user.Role = register.Role;
                }

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User updated successfully: {UserId}", id);
                    return Ok(user);
                }

                var errors = result.Errors.Select(e => e.Description);
                _logger.LogWarning("User update failed: {UserId}, Errors: {Errors}", id, string.Join(", ", errors));
                return BadRequest(new { Errors = errors });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the user: {UserId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var existingUser = await _userRepository.GetUserByIdAsync(id);
                if (existingUser == null)
                {
                    _logger.LogWarning("User not found for deletion: {UserId}", id);
                    return NotFound();
                }

                var result = await _userRepository.DeleteUserAsync(id);
                if (!result)
                {
                    _logger.LogError("Failed to delete user: {UserId}", id);
                    return BadRequest("Failed to delete user.");
                }

                _logger.LogInformation("User deleted successfully: {UserId}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the user: {UserId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }
    }
}
