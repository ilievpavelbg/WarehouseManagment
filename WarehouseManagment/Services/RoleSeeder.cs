using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WarehouseManagment.Constants;
using WarehouseManagment.Data;
using WarehouseManagment.Interfaces;

namespace WarehouseManagment.Services
{
    public class RoleSeeder : IRoleSeeder
    {
        private const string ExistingAdminUserName = "Veteida_Admin";

        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RoleSeeder> _logger;

        public RoleSeeder(
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            ILogger<RoleSeeder> logger)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            foreach (var role in ApplicationRoles.All)
            {
                if (await _roleManager.RoleExistsAsync(role))
                {
                    continue;
                }

                var result = await _roleManager.CreateAsync(new IdentityRole(role));

                if (!result.Succeeded)
                {
                    _logger.LogError("Failed to create role {Role}. Errors: {Errors}", role, string.Join("; ", result.Errors.Select(x => x.Description)));
                }
            }

            var adminUser = await FindExistingAdministratorUserAsync();

            if (adminUser == null)
            {
                _logger.LogWarning("Existing administrator user {UserName} was not found. No administrator account was created automatically.", ExistingAdminUserName);
                return;
            }

            _logger.LogInformation("Existing administrator user {UserName} was found with user id {UserId}.", adminUser.UserName, adminUser.Id);

            if (await _userManager.IsInRoleAsync(adminUser, ApplicationRoles.Administrator))
            {
                _logger.LogInformation("User {UserName} already has the {Role} role.", adminUser.UserName, ApplicationRoles.Administrator);
                return;
            }

            var addRoleResult = await _userManager.AddToRoleAsync(adminUser, ApplicationRoles.Administrator);

            if (!addRoleResult.Succeeded)
            {
                _logger.LogError("Failed to assign {Role} role to {UserName}. Errors: {Errors}", ApplicationRoles.Administrator, ExistingAdminUserName, string.Join("; ", addRoleResult.Errors.Select(x => x.Description)));
                return;
            }

            _logger.LogInformation("Successfully assigned {Role} role to {UserName}. A normal logout and fresh login will include the new role claim.", ApplicationRoles.Administrator, adminUser.UserName);
        }

        private async Task<ApplicationUser?> FindExistingAdministratorUserAsync()
        {
            var adminUser = await _userManager.FindByNameAsync(ExistingAdminUserName);

            if (adminUser != null)
            {
                return adminUser;
            }

            adminUser = await _userManager.FindByEmailAsync(ExistingAdminUserName);

            if (adminUser != null)
            {
                return adminUser;
            }

            var normalizedLookup = ExistingAdminUserName.ToUpperInvariant();
            var matchingUsers = await _userManager.Users
                .Where(x =>
                    x.UserName != null && x.UserName.ToUpper() == normalizedLookup ||
                    x.Email != null && x.Email.ToUpper() == normalizedLookup ||
                    x.NormalizedUserName != null && x.NormalizedUserName == normalizedLookup ||
                    x.NormalizedEmail != null && x.NormalizedEmail == normalizedLookup)
                .ToListAsync();

            if (matchingUsers.Count == 1)
            {
                return matchingUsers[0];
            }

            if (matchingUsers.Count > 1)
            {
                _logger.LogError("More than one user matched {UserName}. Administrator role was not assigned automatically.", ExistingAdminUserName);
            }

            return null;
        }
    }
}
