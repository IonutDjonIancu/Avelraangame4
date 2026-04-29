using Statics;
using System.ComponentModel.DataAnnotations;

namespace Models;

public class Player
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string HashedPassword { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class RegisterRequest
{
    [Required]
    [MinLength(3, ErrorMessage = Errors.NameTooShort)]
    [MaxLength(50, ErrorMessage = Errors.NameTooLong)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MinLength(5, ErrorMessage = Errors.PasswordTooShort)]
    public string Password { get; set; } = string.Empty;
}

public class LoginRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
