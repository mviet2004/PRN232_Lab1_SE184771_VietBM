using System.ComponentModel.DataAnnotations;

namespace PRN232.LMS.API.Models.Requests;

public class CreateEnrollmentRequest
{
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public DateTime EnrollDate { get; set; }

    [Required, MaxLength(20)]
    public string Status { get; set; } = null!;
}
