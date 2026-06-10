using Microsoft.AspNetCore.Identity;

namespace NdlovuBetting.Models
{
    // The login account (email + password). Linked to one User profile.
    public class ApplicationUser : IdentityUser
    {
        // Foreign key to the person's profile/details.
        public int? UserId { get; set; }
        public User? User { get; set; }
    }

    // Role name constants so we don't mistype "Admin"/"Client" anywhere.
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Client = "Client";
    }
}