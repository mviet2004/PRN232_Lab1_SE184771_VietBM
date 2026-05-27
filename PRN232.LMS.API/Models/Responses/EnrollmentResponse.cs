namespace PRN232.LMS.API.Models.Responses;

public class EnrollmentResponse
{
    public int EnrollmentId { get; set; }
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public DateTime EnrollDate { get; set; }
    public string Status { get; set; } = null!;
    public EnrollmentStudentBriefResponse? Student { get; set; }
    public EnrollmentCourseBriefResponse? Course { get; set; }
}

public class EnrollmentStudentBriefResponse
{
    public int StudentId { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
}

public class EnrollmentCourseBriefResponse
{
    public int CourseId { get; set; }
    public string CourseName { get; set; } = null!;
    public int SemesterId { get; set; }
}
