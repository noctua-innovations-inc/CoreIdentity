#nullable disable

using System.ComponentModel.DataAnnotations;

namespace AspNetIdentity.Models;

public class UserLogin
{
    [Display(Name = "User Name")]
    [Required]
    public string UserName { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [Display(AutoGenerateField = false)]
    public string Token { get; set; }
}