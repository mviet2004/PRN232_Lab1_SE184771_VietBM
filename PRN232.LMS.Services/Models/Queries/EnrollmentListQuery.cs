namespace PRN232.LMS.Services.Models.Queries;

public class EnrollmentListQuery
{
    public string? Search { get; set; }
    public string? Sort { get; set; }
    public int? StudentId { get; set; }
    public int? CourseId { get; set; }
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 10;
    public string? Fields { get; set; }
    public string? Expand { get; set; }
}
