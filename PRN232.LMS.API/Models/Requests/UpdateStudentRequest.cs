using System.ComponentModel.DataAnnotations;

namespace PRN232.LMS.API.Models.Requests;

public class UpdateStudentRequest
{
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = null!;

    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = null!;

    [Required]
    public DateTime DateOfBirth { get; set; }
}
