using desco_report_server.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace desco_report_server.Data
{
    public class DatabaseSeeder
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<DatabaseSeeder> _logger;

        public DatabaseSeeder(
            ApplicationDbContext context,
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<DatabaseSeeder> logger)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            try
            {
                await _context.Database.MigrateAsync();
                await SeedRolesAsync();
                await SeedAdminUserAsync();
                _logger.LogInformation("Database seeding completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while seeding the database");
                throw;
            }
        }

        private async Task SeedRolesAsync()
        {
            var roles = new[] { "Admin", "User", "Manager" };

            foreach (var roleName in roles)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    await _roleManager.CreateAsync(new IdentityRole(roleName));
                    _logger.LogInformation("Created role: {RoleName}", roleName);
                }
            }
        }

        private async Task SeedAdminUserAsync()
        {
            const string adminEmail = "admin@desco.com";
            const string adminUsername = "admin";
            const string adminPassword = "Admin@123";

            var existingAdmin = await _userManager.FindByEmailAsync(adminEmail);
            if (existingAdmin == null)
            {
                var adminUser = new User
                {
                    UserName = adminUsername,
                    Email = adminEmail,
                    FirstName = "System",
                    LastName = "Administrator",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(adminUser, "Admin");
                    _logger.LogInformation("Created admin user: {Email}", adminEmail);
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogError("Failed to create admin user: {Errors}", errors);
                }
            }
        }
    }
}