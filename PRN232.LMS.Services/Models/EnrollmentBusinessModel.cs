namespace PRN232.LMS.Services.Models;

public class EnrollmentBusinessModel
{
    public int EnrollmentId { get; set; }
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public DateTime EnrollDate { get; set; }
    public string Status { get; set; } = null!;
    public EnrollmentStudentBriefBusinessModel? Student { get; set; }
    public EnrollmentCourseBriefBusinessModel? Course { get; set; }
}

public class EnrollmentStudentBriefBusinessModel
{
    public int StudentId { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
}

public class EnrollmentCourseBriefBusinessModel
{
    public int CourseId { get; set; }
    public string CourseName { get; set; } = null!;
    public int SemesterId { get; set; }
}
