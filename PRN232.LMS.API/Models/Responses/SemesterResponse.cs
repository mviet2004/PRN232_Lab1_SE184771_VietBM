namespace PRN232.LMS.API.Models.Responses;

public class SemesterResponse
{
    public int SemesterId { get; set; }
    public string SemesterName { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<SemesterCourseBriefResponse>? Courses { get; set; }
}

public class SemesterCourseBriefResponse
{
    public int CourseId { get; set; }
    public string CourseName { get; set; } = null!;
    public int SemesterId { get; set; }
}
