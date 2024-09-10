using MicroServicePatient.Models;
using Microsoft.AspNetCore.Identity;

namespace MicroServicePatient.Services
{
    public class RoleSeeder
    {
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<RoleSeeder> _logger;

        public RoleSeeder(RoleManager<IdentityRole<int>> roleManager, UserManager<User> userManager, ILogger<RoleSeeder> logger)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task SeedRolesAsync()
        {
            string[] roleNames = { "Admin", "User" };

            foreach (var roleName in roleNames)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    var roleResult = await _roleManager.CreateAsync(new IdentityRole<int>(roleName));
                    if (roleResult.Succeeded)
                    {
                        _logger.LogInformation("Role {RoleName} created", roleName);
                    }
                    else
                    {
                        _logger.LogError("Error creating role {RoleName}: {Errors}", roleName, roleResult.Errors);
                    }
                }
            }
        }
    }
}
