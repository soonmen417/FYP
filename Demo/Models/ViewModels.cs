using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Demo.Models;

// View Models ----------------------------------------------------------------

#nullable disable warnings

public class LoginVM
{
    [StringLength(100)]
    [EmailAddress]
    public string Email { get; set; }

    [StringLength(100, MinimumLength = 5)]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    public bool RememberMe { get; set; }

    public static LoginVM PreviousUser { get; set; }

    public static LoginVM SignedInUser { get; set; }

    public bool TwoFactorEnabled { get; set; }
}

public class RegisterVM
{

    [StringLength(100)]
    [EmailAddress]
    [Remote("CheckEmail", "Account", ErrorMessage = "Duplicated {0}.")]
    public string Email { get; set; }

    [StringLength(100, MinimumLength = 10, ErrorMessage = "{0} must be {2}-{1} characters long.")]
    [RegularExpression(@"[a-zA-Z][a-zA-Z ]{2,}", ErrorMessage = "Invalid Format")]
    public string Name { get; set; }

    [MaxLength(10)]
    public string Gender { get; set; }

    [MaxLength(20)]
    [RegularExpression(@"\+601\d-\d{7,8}", ErrorMessage = "Invalid {0} Numbers. Ex: +601X-XXXXXXXX")]
    public string Phone { get; set; }

    [MaxLength(200)]
    public string Address { get; set; }

    [MaxLength(20)]
    [Display(Name = "Intake Year")]
    public string IntakeYear { get; set; }

    [MaxLength(20)]
    public string Faculty { get; set; }

    [MaxLength(20)]
    public string Programme { get; set; }

    [StringLength(100, MinimumLength = 5, ErrorMessage = "{0} must be {2}-{1} characters long.")]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [StringLength(100, MinimumLength = 5, ErrorMessage = "{0} must be {2}-{1} characters long.")]
    [Compare("Password")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    public string Confirm { get; set; }

    public IFormFile Photo { get; set; }
}

public class SecuirtyCodeVM
{
    [StringLength(100)]
    [EmailAddress]
    [Remote("CheckEmail", "Account", ErrorMessage = "Duplicated {0}.")]
    public string Email { get; set; }

    [MaxLength(20)]
    [RegularExpression(@"\+601\d-\d{7,8}", ErrorMessage = "Invalid {0} Numbers. Ex: +601X-XXXXXXXX")]
    public string? Phone { get; set; }

    [MaxLength(30)]
    public string SecurityCode { get; set; }
}

public class UpdateProfileVM
{
    public string? Email { get; set; }

    [StringLength(100)]
    public string Name { get; set; }

    [MaxLength(20)]
    public string Phone { get; set; }

    [MaxLength(200)]
    public string Address { get; set; }

    public string? PhotoURL { get; set; }

    public IFormFile? Photo { get; set; }
}

public class MaintenanceVM
{
    public string? Email { get; set; }

    [StringLength(100)]
    public string Name { get; set; }

    [MaxLength(10)]
    public string Gender { get; set; }

    [MaxLength(20)]
    public string Phone { get; set; }

    [MaxLength(200)]
    public string Address { get; set; }
}

public class UpdatePasswordVM
{
    [StringLength(100, MinimumLength = 5, ErrorMessage = "{0} must be {2}-{1} characters long.")]
    [DataType(DataType.Password)]
    [Display(Name = "Current Password")]
    public string Current { get; set; }

    [StringLength(100, MinimumLength = 5, ErrorMessage = "{0} must be {2}-{1} characters long.")]
    [DataType(DataType.Password)]
    [Display(Name = "New Password")]
    public string New { get; set; }

    [StringLength(100, MinimumLength = 5, ErrorMessage = "{0} must be {2}-{1} characters long.")]
    [Compare("New")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    public string Confirm { get; set; }
}

public class ResetPasswordVM
{
    [StringLength(100)]
    [EmailAddress]
    public string Email { get; set; }
}

// NEW ------------------------------------------------------------------------

public class EditProfileVM
{
    [StringLength(100)]
    [Remote("CheckEmail", "Account", ErrorMessage = "Duplicated {0}.")]
    [RegularExpression(@"^[a-zA-Z0-9+_.-]+@[a-zA-Z0-9.-]+$", ErrorMessage = "Invalid {0} Format.")]
    public string Email { get; set; }

    [StringLength(100, MinimumLength = 10, ErrorMessage = "{0} must be {2}-{1} characters long.")]
    [RegularExpression(@"[a-zA-Z][a-zA-Z ]{2,}", ErrorMessage = "Invalid {0} Format.")]
    public string Name { get; set; }

    [MaxLength(20)]
    [RegularExpression(@"\+601\d-\d{7,8}", ErrorMessage = "Invalid {0} Numbers. Ex: +601X-XXXXXXXX")]
    public string Phone { get; set; }

    [MaxLength(200)]
    public string Address { get; set; }

    [MaxLength(10)]
    public string Role { get; set; }

    [MaxLength(20)]
    [Display(Name = "Intake Year")]
    public string? IntakeYear { get; set; }

    [MaxLength(20)]
    public string? Faculty { get; set; }

    [MaxLength(20)]
    public string? Programme { get; set; }
}

public class InsertVM
{
    [StringLength(100)]
    [EmailAddress]
    [Remote("CheckEmail", "Account", ErrorMessage = "Duplicated {0}.")]
    public string Email { get; set; }

    [StringLength(100, MinimumLength = 10, ErrorMessage = "{0} must be {2}-{1} characters long.")]
    [RegularExpression(@"[a-zA-Z][a-zA-Z ]{2,}", ErrorMessage = "Invalid Format")]
    public string Name { get; set; }

    [MaxLength(10)]
    public string Gender { get; set; }

    [MaxLength(20)]
    [RegularExpression(@"\+601\d-\d{7,8}", ErrorMessage = "Invalid {0} Numbers. Ex: +601X-XXXXXXXX")]
    public string Phone { get; set; }

    [MaxLength(200)]
    public string Address { get; set; }

    [MaxLength(20)]
    [Display(Name = "Intake Year")]
    public string? IntakeYear { get; set; }

    [MaxLength(20)]
    public string? Faculty { get; set; }

    [MaxLength(20)]
    public string? Programme { get; set; }

    [StringLength(100, MinimumLength = 5, ErrorMessage = "{0} must be {2}-{1} characters long.")]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [StringLength(100, MinimumLength = 5, ErrorMessage = "{0} must be {2}-{1} characters long.")]
    [Compare("Password")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    public string Confirm { get; set; }

    public IFormFile Photo { get; set; }

    [MaxLength(10)]
    public string Role { get; set; }
}

public class RoomInsertVM
{
    [StringLength(15)]
    [Display(Name = "Room ID")]
    [RegularExpression(@"R\d{3}", ErrorMessage = "Invalid {0} format.")]
    [Remote("CheckRoomId", "Room", ErrorMessage = "Duplicated {0}.")]
    public string RoomId { get; set; }

    [StringLength(10, MinimumLength = 3, ErrorMessage = "{0} must be {2}-{1} characters long.")]
    [Display(Name = "Room Name")]
    [RegularExpression(@"^[a-zA-Z0-9][a-zA-Z0-9 ]*$", ErrorMessage = "Invalid Format")]
    public string Room_Name { get; set; }

    [MaxLength(50)]
    [Display(Name = "Room Description")]
    public string Room_Description { get; set; }

    [MaxLength(10)]
    [Display(Name = "Room Type")]
    public string Room_Type { get; set; }
}

public class EditRoomVM
{
    [StringLength(15)]
    [Display(Name = "Room ID")]
    [RegularExpression(@"R\d{3}", ErrorMessage = "Invalid {0} format.")]
    public string RoomId { get; set; }

    [StringLength(10, MinimumLength = 3, ErrorMessage = "{0} must be {2}-{1} characters long.")]
    [Display(Name = "Room Name")]
    [RegularExpression(@"^[a-zA-Z0-9][a-zA-Z0-9 ]*$", ErrorMessage = "Invalid Format")]
    public string Room_Name { get; set; }

    [MaxLength(50)]
    [Display(Name = "Room Description")]
    public string Room_Description { get; set; }

    [MaxLength(10)]
    [Display(Name = "Room Type")]
    public string Room_Type { get; set; }
}

public class ChatVM
{
    public string Id { get; set; }

    public string Name { get; set; }

    public string PhotoURL { get; set; }

    public string Message { get; set; }

    public IFormFile? Photo { get; set; }
}

public class EnterChatVM
{
    [StringLength(15)]
    [Display(Name = "Room ID")]
    [RegularExpression(@"R\d{3}", ErrorMessage = "Invalid {0} format.")]
    public string RoomId { get; set; }
}