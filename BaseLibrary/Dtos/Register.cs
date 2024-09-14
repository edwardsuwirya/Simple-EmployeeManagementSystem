using System.ComponentModel.DataAnnotations;

namespace BaseLibrary.Dtos;

public class Register : AuthBase
{
    [Required]
    [MinLength(5)]
    [MaxLength(75)]
    public string? FullName { get; set; }

    [DataType(DataType.Password)]
    [Compare(nameof(Password))]
    [Required]
    public string? ConfirmPassword { get; set; }
}