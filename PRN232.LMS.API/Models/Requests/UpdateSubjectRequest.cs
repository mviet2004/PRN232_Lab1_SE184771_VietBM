using System.ComponentModel.DataAnnotations;

namespace PRN232.LMS.API.Models.Requests;

public class UpdateSubjectRequest
{
    [Required, MaxLength(20)]
    public string SubjectCode { get; set; } = null!;

    [Required, MaxLength(100)]
    public string SubjectName { get; set; } = null!;

    public int Credit { get; set; }
}
