using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class RegisterInput
{
    public string ReturnUrl {get;set;}
    
    [StringLength(64, MinimumLength = 4, ErrorMessage = "Too short or long.")]
    [Required]
    public string Username { get; set;}

    [StringLength(64, MinimumLength = 8, ErrorMessage = "Too short or long.")]
    [Required]
    public string Password {get;set;}

    [Compare("Password")]
    [Required]
    public string PasswordAgain {get;set;}
}

public class RegisterResult
{
    public string ReturnUrl {get;set;}
}

