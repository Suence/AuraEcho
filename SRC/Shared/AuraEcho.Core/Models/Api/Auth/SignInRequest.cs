using System.ComponentModel.DataAnnotations;

namespace AuraEcho.Core.Models.Api;

public class SignInRequest
{
    [Required]
    public string UserName { get; set; }

    [Required]
    public string Password { get; set; }
}