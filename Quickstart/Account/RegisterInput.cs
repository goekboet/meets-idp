using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

public class RegisterInput
{
    public string ReturnUrl { get; set; }

    public bool RememberMe { get; set; }

    [EmailAddress]
    [Required]
    public string Email { get; set; }
    public string UserName => new string(Email.TakeWhile(x => x != '@').ToArray());

    [StringLength(64, MinimumLength = 8, ErrorMessage = "Too short or long.")]
    [Required]
    public string Password { get; set; }

    [Compare("Password")]
    [Required]
    public string PasswordAgain { get; set; }
}

public class RegisterResult
{
    public string ReturnUrl { get; set; }
}

