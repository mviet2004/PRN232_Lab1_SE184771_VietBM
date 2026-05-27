namespace PRN232.LMS.Services.Models;

public class SemesterBusinessModel
{
    public int SemesterId { get; set; }
    public string SemesterName { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<SemesterCourseBriefBusinessModel>? Courses { get; set; }
}

public class SemesterCourseBriefBusinessModel
{
    public int CourseId { get; set; }
    public string CourseName { get; set; } = null!;
    public int SemesterId { get; set; }
}
