using System.ComponentModel.DataAnnotations;

namespace NdlovuBetting.Models
{
    // A person's profile/details. The admin manages these; a client owns one.
    public class User
    {
        public int Id { get; set; }

        // ID Number is unique - a person can only sign up once with the same ID.
        [Required]
        [MaxLength(13)]
        [Display(Name = "ID Number")]
        public string IdNumber { get; set; } = "";

        [Required]
        [MaxLength(100)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = "";

        [Required]
        [MaxLength(100)]
        public string Surname { get; set; } = "";

        [MaxLength(150)]
        [EmailAddress]
        public string? Email { get; set; }

        [MaxLength(20)]
        [Phone]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [MaxLength(250)]
        public string? Address { get; set; }

        public DateTime CreatedAt { get; set; }

        // A user owns one wallet account (we allow a list for flexibility).
        public List<Account> Accounts { get; set; } = new();
    }
}