using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Services.Models;

namespace PRN232.LMS.Services.Mappings;

public static class StudentMappings
{
    public static StudentBusinessModel ToBusinessModel(this Student entity, bool includeEnrollments = false)
    {
        var model = new StudentBusinessModel
        {
            StudentId = entity.StudentId,
            FullName = entity.FullName,
            Email = entity.Email,
            DateOfBirth = entity.DateOfBirth
        };

        if (includeEnrollments && entity.Enrollments.Count > 0)
        {
            model.Enrollments = entity.Enrollments.Select(e => new EnrollmentBriefBusinessModel
            {
                EnrollmentId = e.EnrollmentId,
                CourseId = e.CourseId,
                EnrollDate = e.EnrollDate,
                Status = e.Status
            }).ToList();
        }

        return model;
    }

    public static Student ToEntity(this StudentBusinessModel model) =>
        new()
        {
            StudentId = model.StudentId,
            FullName = model.FullName,
            Email = model.Email,
            DateOfBirth = model.DateOfBirth
        };

    public static void UpdateEntity(this Student entity, StudentBusinessModel model)
    {
        entity.FullName = model.FullName;
        entity.Email = model.Email;
        entity.DateOfBirth = model.DateOfBirth;
    }
}
