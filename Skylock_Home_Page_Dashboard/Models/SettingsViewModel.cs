using System.ComponentModel.DataAnnotations;

public class SettingsViewModel
{
    // For username update
    [Required(ErrorMessage = "Username is required")]
    [Display(Name = "Username")]
    public string Username { get; set; }

    // For password change
    [DataType(DataType.Password)]
    [Display(Name = "Current Password")]
    public string? CurrentPassword { get; set; }  // Make nullable

    [DataType(DataType.Password)]
    [Display(Name = "New Password")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    public string? NewPassword { get; set; }  // Make nullable

    [DataType(DataType.Password)]
    [Display(Name = "Confirm New Password")]
    [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
    public string? ConfirmPassword { get; set; }  // Make nullable
}

// UsernameUpdateViewModel.cs
public class UsernameUpdateViewModel
{
    [Required(ErrorMessage = "Username is required")]
    public string Username { get; set; }
}

// PasswordUpdateViewModel.cs
public class PasswordUpdateViewModel
{
    [Required(ErrorMessage = "Current password is required")]
    public string CurrentPassword { get; set; }

    [Required(ErrorMessage = "New password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    public string NewPassword { get; set; }

    [Required(ErrorMessage = "Please confirm your password")]
    [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; }
}