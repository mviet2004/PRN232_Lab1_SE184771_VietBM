namespace PRN232.LMS.Services.Models;

public class CourseBusinessModel
{
    public int CourseId { get; set; }
    public string CourseName { get; set; } = null!;
    public int SemesterId { get; set; }
    public CourseSemesterBriefBusinessModel? Semester { get; set; }
    public List<CourseEnrollmentBriefBusinessModel>? Enrollments { get; set; }
}

public class CourseSemesterBriefBusinessModel
{
    public int SemesterId { get; set; }
    public string SemesterName { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class CourseEnrollmentBriefBusinessModel
{
    public int EnrollmentId { get; set; }
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public DateTime EnrollDate { get; set; }
    public string Status { get; set; } = null!;
}
