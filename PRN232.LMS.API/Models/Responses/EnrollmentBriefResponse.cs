using System.Text.Json.Serialization;

namespace PRN232.LMS.API.Models.Responses;

public class EnrollmentBriefResponse
{
    public int EnrollmentId { get; set; }
    public int CourseId { get; set; }
    public DateTime EnrollDate { get; set; }
    public string Status { get; set; } = null!;
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public EnrollmentCourseBriefResponse? Course { get; set; }
}
