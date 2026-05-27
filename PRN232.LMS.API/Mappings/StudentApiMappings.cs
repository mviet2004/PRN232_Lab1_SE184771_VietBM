using PRN232.LMS.API.Models.Requests;
using PRN232.LMS.API.Models.Responses;
using PRN232.LMS.Services.Models;

namespace PRN232.LMS.API.Mappings;

public static class StudentApiMappings
{
    public static StudentBusinessModel ToBusinessModel(this CreateStudentRequest request) =>
        new()
        {
            FullName = request.FullName,
            Email = request.Email,
            DateOfBirth = request.DateOfBirth
        };

    public static StudentBusinessModel ToBusinessModel(this UpdateStudentRequest request) =>
        new()
        {
            FullName = request.FullName,
            Email = request.Email,
            DateOfBirth = request.DateOfBirth
        };

    public static StudentResponse ToResponse(this StudentBusinessModel model) =>
        new()
        {
            StudentId = model.StudentId,
            FullName = model.FullName,
            Email = model.Email,
            DateOfBirth = model.DateOfBirth,
            Enrollments = model.Enrollments?.Select(e => new EnrollmentBriefResponse
            {
                EnrollmentId = e.EnrollmentId,
                CourseId = e.CourseId,
                EnrollDate = e.EnrollDate,
                Status = e.Status
            }).ToList()
        };

    public static object ToResponseObject(this StudentBusinessModel model, string? fields = null)
    {
        var response = model.ToResponse();
        if (string.IsNullOrWhiteSpace(fields))
        {
            return response;
        }

        var allowed = fields
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(f => f.ToLowerInvariant())
            .ToHashSet();

        var result = new Dictionary<string, object?>();
        if (allowed.Contains("studentid")) result["studentId"] = response.StudentId;
        if (allowed.Contains("fullname")) result["fullName"] = response.FullName;
        if (allowed.Contains("email")) result["email"] = response.Email;
        if (allowed.Contains("dateofbirth")) result["dateOfBirth"] = response.DateOfBirth;
        if (allowed.Contains("enrollments")) result["enrollments"] = response.Enrollments;

        return result;
    }
}
