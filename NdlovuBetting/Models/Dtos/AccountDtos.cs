using System.ComponentModel.DataAnnotations;

namespace NdlovuBetting.Models.Dtos
{
    // Data the signup form collects.
    public class RegisterDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required, DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
        public string Password { get; set; } = "";

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = "";

        [Required, MaxLength(13)]
        [RegularExpression(@"^\d{13}$", ErrorMessage = "ID Number must be 13 digits.")]
        [Display(Name = "ID Number")]
        public string IdNumber { get; set; } = "";

        [Required, MaxLength(100)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = "";

        [Required, MaxLength(100)]
        public string Surname { get; set; } = "";

        [MaxLength(20), Phone]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [Required, MaxLength(250)]
        public string Address { get; set; } = "";
    }

    // Data the login form collects.
    public class LoginDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }

    // What the deposit/withdraw form in the wallet popup submits.
    public enum WalletAction
    {
        Deposit = 0,
        Withdraw = 1
    }

    public class WalletActionDto
    {
        [Required]
        public WalletAction Action { get; set; } = WalletAction.Deposit;

        [Required]
        [Range(0.01, 1000000, ErrorMessage = "Amount must be greater than zero.")]
        public decimal Amount { get; set; }
    }

    // Data the Edit Profile form submits (in the profile popup).
    public class ProfileDto
    {
        [Required, MaxLength(100)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = "";

        [Required, MaxLength(100)]
        public string Surname { get; set; } = "";

        [MaxLength(150), EmailAddress]
        public string? Email { get; set; }

        [MaxLength(20), Phone]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [MaxLength(250)]
        public string? Address { get; set; }
    }
}