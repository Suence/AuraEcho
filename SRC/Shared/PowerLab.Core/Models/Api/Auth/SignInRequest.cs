using System.ComponentModel.DataAnnotations;

namespace PowerLab.Core.Models.Api;

public class SignInRequest
{
    [Required]
    public string UserName { get; set; }

    [Required]
    public string Password { get; set; }
}