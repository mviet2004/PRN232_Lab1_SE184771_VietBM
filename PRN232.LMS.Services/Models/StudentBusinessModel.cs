namespace PRN232.LMS.Services.Models;

public class StudentBusinessModel
{
    public int StudentId { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public DateTime DateOfBirth { get; set; }
    public List<EnrollmentBriefBusinessModel>? Enrollments { get; set; }
}
