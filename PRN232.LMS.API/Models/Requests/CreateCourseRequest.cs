using System.ComponentModel.DataAnnotations;

namespace PRN232.LMS.API.Models.Requests;

public class CreateCourseRequest
{
    [Required, MaxLength(100)]
    public string CourseName { get; set; } = null!;

    public int SemesterId { get; set; }
}
