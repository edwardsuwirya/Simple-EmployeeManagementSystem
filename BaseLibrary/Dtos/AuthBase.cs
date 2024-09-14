using System.ComponentModel.DataAnnotations;

namespace BaseLibrary.Dtos;

public class AuthBase
{
    [EmailAddress]
    [Required]
    [DataType(DataType.EmailAddress)]
    public string? Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public string? Password { get; set; }
}