namespace PRN232.LMS.Services.Models;

public class EnrollmentBriefBusinessModel
{
    public int EnrollmentId { get; set; }
    public int CourseId { get; set; }
    public DateTime EnrollDate { get; set; }
    public string Status { get; set; } = null!;
}
