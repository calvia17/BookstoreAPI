using Microsoft.AspNetCore.Identity;

namespace RabbitHoleService.Objects
{
    /// <summary>
    /// The application user class.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// A value indicating whether the user is deleted.
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// The name of the user.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationUser" /> class.
        /// </summary>
        /// <param name="name">The name of the user.</param>
        public ApplicationUser(string name) : base()
        {
            this.IsDeleted = false;
            this.Name = name;
        }
    }
}
