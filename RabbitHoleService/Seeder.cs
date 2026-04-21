using Microsoft.AspNetCore.Identity;
using RabbitHoleService.Objects;

namespace RabbitHoleService
{
    /// <summary>
    /// The seeder.
    /// </summary>
    public class Seeder
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="Seeder" /> class.
        /// </summary>
        /// <param name="userManager">The user manager.</param>
        /// <param name="roleManager">The role manager.</param>
        /// <param name="configuration">The configuration.</param>
        public Seeder(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.configuration = configuration;
        }

        /// <summary>
        /// Seeds the roles if they do not exist.
        /// </summary>
        /// <returns>A task representing the role seed operation.</returns>
        public async Task SeedRolesAsync()
        {
            string[] roleNames = { RoleType.Admin.ToString(), RoleType.Customer.ToString(), RoleType.Staff.ToString() };
            foreach (var roleName in roleNames)
            {
                if (!await this.roleManager.RoleExistsAsync(roleName))
                {
                    await this.roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }

        /// <summary>
        /// Seeds the admin user if it does not exist.
        /// </summary>
        /// <returns>A task representing the admin seed operation.</returns>
        public async Task SeedAdminAsync()
        {
            var admins = this.userManager.GetUsersInRoleAsync(RoleType.Admin.ToString()).Result;
            if (admins.Count > 0)
            {
                return;
            }

            var adminEmail = this.configuration["Admin:Email"];
            var adminPassword = this.configuration["Admin:Password"];

            if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
            {
                throw new InvalidOperationException("Admin email and password must be provided.");
            }

            var adminUser = new ApplicationUser("Admin")
            {
                UserName = adminEmail,
                Email = adminEmail
            };

            var result = await this.userManager.CreateAsync(adminUser, adminPassword);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException("Failed to create admin.");
            }

            await this.userManager.AddToRoleAsync(adminUser, RoleType.Admin.ToString());
        }
    }
}
