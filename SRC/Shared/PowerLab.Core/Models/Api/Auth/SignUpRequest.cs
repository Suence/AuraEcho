using System.ComponentModel.DataAnnotations;

namespace PowerLab.Core.Models.Api;

public class SignUpRequest
{
    [Required]
    public string UserName { get; set; }

    [Required]
    public string Password { get; set; }
}
