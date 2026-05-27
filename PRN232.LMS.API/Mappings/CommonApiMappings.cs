using PRN232.LMS.API.Models.Requests;
using PRN232.LMS.API.Models.Responses;
using PRN232.LMS.Services.Models;

namespace PRN232.LMS.API.Mappings;

public static class CommonApiMappings
{
    public static SemesterBusinessModel ToBusinessModel(this CreateSemesterRequest request) =>
        new()
        {
            SemesterName = request.SemesterName,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };

    public static SemesterBusinessModel ToBusinessModel(this UpdateSemesterRequest request) =>
        new()
        {
            SemesterName = request.SemesterName,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };

    public static SemesterResponse ToResponse(this SemesterBusinessModel model) =>
        new()
        {
            SemesterId = model.SemesterId,
            SemesterName = model.SemesterName,
            StartDate = model.StartDate,
            EndDate = model.EndDate,
            Courses = model.Courses?.Select(c => new SemesterCourseBriefResponse
            {
                CourseId = c.CourseId,
                CourseName = c.CourseName,
                SemesterId = c.SemesterId
            }).ToList()
        };

    public static object ToResponseObject(this SemesterBusinessModel model, string? fields = null)
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
        if (allowed.Contains("semesterid")) result["semesterId"] = response.SemesterId;
        if (allowed.Contains("semestername")) result["semesterName"] = response.SemesterName;
        if (allowed.Contains("startdate")) result["startDate"] = response.StartDate;
        if (allowed.Contains("enddate")) result["endDate"] = response.EndDate;
        if (allowed.Contains("courses")) result["courses"] = response.Courses;

        return result;
    }

    public static CourseBusinessModel ToBusinessModel(this CreateCourseRequest request) =>
        new()
        {
            CourseName = request.CourseName,
            SemesterId = request.SemesterId
        };

    public static CourseBusinessModel ToBusinessModel(this UpdateCourseRequest request) =>
        new()
        {
            CourseName = request.CourseName,
            SemesterId = request.SemesterId
        };

    public static CourseResponse ToResponse(this CourseBusinessModel model) =>
        new()
        {
            CourseId = model.CourseId,
            CourseName = model.CourseName,
            SemesterId = model.SemesterId,
            Semester = model.Semester is null
                ? null
                : new CourseSemesterBriefResponse
                {
                    SemesterId = model.Semester.SemesterId,
                    SemesterName = model.Semester.SemesterName,
                    StartDate = model.Semester.StartDate,
                    EndDate = model.Semester.EndDate
                },
            Enrollments = model.Enrollments?.Select(e => new CourseEnrollmentBriefResponse
            {
                EnrollmentId = e.EnrollmentId,
                StudentId = e.StudentId,
                CourseId = e.CourseId,
                EnrollDate = e.EnrollDate,
                Status = e.Status
            }).ToList()
        };

    public static object ToResponseObject(this CourseBusinessModel model, string? fields = null)
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
        if (allowed.Contains("courseid")) result["courseId"] = response.CourseId;
        if (allowed.Contains("coursename")) result["courseName"] = response.CourseName;
        if (allowed.Contains("semesterid")) result["semesterId"] = response.SemesterId;
        if (allowed.Contains("semester")) result["semester"] = response.Semester;
        if (allowed.Contains("enrollments")) result["enrollments"] = response.Enrollments;

        return result;
    }

    public static SubjectBusinessModel ToBusinessModel(this CreateSubjectRequest request) =>
        new()
        {
            SubjectCode = request.SubjectCode,
            SubjectName = request.SubjectName,
            Credit = request.Credit
        };

    public static SubjectBusinessModel ToBusinessModel(this UpdateSubjectRequest request) =>
        new()
        {
            SubjectCode = request.SubjectCode,
            SubjectName = request.SubjectName,
            Credit = request.Credit
        };

    public static SubjectResponse ToResponse(this SubjectBusinessModel model) =>
        new()
        {
            SubjectId = model.SubjectId,
            SubjectCode = model.SubjectCode,
            SubjectName = model.SubjectName,
            Credit = model.Credit
        };

    public static object ToResponseObject(this SubjectBusinessModel model, string? fields = null)
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
        if (allowed.Contains("subjectid")) result["subjectId"] = response.SubjectId;
        if (allowed.Contains("subjectcode")) result["subjectCode"] = response.SubjectCode;
        if (allowed.Contains("subjectname")) result["subjectName"] = response.SubjectName;
        if (allowed.Contains("credit")) result["credit"] = response.Credit;

        return result;
    }

    public static EnrollmentBusinessModel ToBusinessModel(this CreateEnrollmentRequest request) =>
        new()
        {
            StudentId = request.StudentId,
            CourseId = request.CourseId,
            EnrollDate = request.EnrollDate,
            Status = request.Status
        };

    public static EnrollmentBusinessModel ToBusinessModel(this UpdateEnrollmentRequest request) =>
        new()
        {
            StudentId = request.StudentId,
            CourseId = request.CourseId,
            EnrollDate = request.EnrollDate,
            Status = request.Status
        };

    public static EnrollmentResponse ToResponse(this EnrollmentBusinessModel model) =>
        new()
        {
            EnrollmentId = model.EnrollmentId,
            StudentId = model.StudentId,
            CourseId = model.CourseId,
            EnrollDate = model.EnrollDate,
            Status = model.Status,
            Student = model.Student is null
                ? null
                : new EnrollmentStudentBriefResponse
                {
                    StudentId = model.Student.StudentId,
                    FullName = model.Student.FullName,
                    Email = model.Student.Email
                },
            Course = model.Course is null
                ? null
                : new EnrollmentCourseBriefResponse
                {
                    CourseId = model.Course.CourseId,
                    CourseName = model.Course.CourseName,
                    SemesterId = model.Course.SemesterId
                }
        };

    public static object ToResponseObject(this EnrollmentBusinessModel model, string? fields = null)
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
        if (allowed.Contains("enrollmentid")) result["enrollmentId"] = response.EnrollmentId;
        if (allowed.Contains("studentid")) result["studentId"] = response.StudentId;
        if (allowed.Contains("courseid")) result["courseId"] = response.CourseId;
        if (allowed.Contains("enrolldate")) result["enrollDate"] = response.EnrollDate;
        if (allowed.Contains("status")) result["status"] = response.Status;
        if (allowed.Contains("student")) result["student"] = response.Student;
        if (allowed.Contains("course")) result["course"] = response.Course;

        return result;
    }
}
