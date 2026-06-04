using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Services.Models;

namespace PRN232.LMS.Services.Mappings;

public static class CommonEntityMappings
{
    public static SemesterBusinessModel ToBusinessModel(this Semester entity, bool includeCourses = false)
    {
        var model = new SemesterBusinessModel
        {
            SemesterId = entity.SemesterId,
            SemesterName = entity.SemesterName,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate
        };

        if (includeCourses && entity.Courses.Count > 0)
        {
            model.Courses = entity.Courses.Select(c => new SemesterCourseBriefBusinessModel
            {
                CourseId = c.CourseId,
                CourseName = c.CourseName,
                SemesterId = c.SemesterId
            }).ToList();
        }

        return model;
    }

    public static Semester ToEntity(this SemesterBusinessModel model) =>
        new()
        {
            SemesterName = model.SemesterName,
            StartDate = model.StartDate,
            EndDate = model.EndDate
        };

    public static void UpdateEntity(this Semester entity, SemesterBusinessModel model)
    {
        entity.SemesterName = model.SemesterName;
        entity.StartDate = model.StartDate;
        entity.EndDate = model.EndDate;
    }

    public static CourseBusinessModel ToBusinessModel(
        this Course entity,
        bool includeSemester = false,
        bool includeEnrollments = false)
    {
        var model = new CourseBusinessModel
        {
            CourseId = entity.CourseId,
            CourseName = entity.CourseName,
            SemesterId = entity.SemesterId
        };

        if (includeSemester && entity.Semester is not null)
        {
            model.Semester = new CourseSemesterBriefBusinessModel
            {
                SemesterId = entity.Semester.SemesterId,
                SemesterName = entity.Semester.SemesterName,
                StartDate = entity.Semester.StartDate,
                EndDate = entity.Semester.EndDate
            };
        }

        if (includeEnrollments && entity.Enrollments.Count > 0)
        {
            model.Enrollments = entity.Enrollments.Select(e => new CourseEnrollmentBriefBusinessModel
            {
                EnrollmentId = e.EnrollmentId,
                StudentId = e.StudentId,
                CourseId = e.CourseId,
                EnrollDate = e.EnrollDate,
                Status = e.Status,
                Student = e.Student is not null
                    ? new EnrollmentStudentBriefBusinessModel
                    {
                        StudentId = e.Student.StudentId,
                        FullName = e.Student.FullName,
                        Email = e.Student.Email
                    }
                    : null
            }).ToList();
        }

        return model;
    }

    public static Course ToEntity(this CourseBusinessModel model) =>
        new()
        {
            CourseName = model.CourseName,
            SemesterId = model.SemesterId
        };

    public static void UpdateEntity(this Course entity, CourseBusinessModel model)
    {
        entity.CourseName = model.CourseName;
        entity.SemesterId = model.SemesterId;
    }

    public static SubjectBusinessModel ToBusinessModel(this Subject entity) =>
        new()
        {
            SubjectId = entity.SubjectId,
            SubjectCode = entity.SubjectCode,
            SubjectName = entity.SubjectName,
            Credit = entity.Credit
        };

    public static Subject ToEntity(this SubjectBusinessModel model) =>
        new()
        {
            SubjectCode = model.SubjectCode,
            SubjectName = model.SubjectName,
            Credit = model.Credit
        };

    public static void UpdateEntity(this Subject entity, SubjectBusinessModel model)
    {
        entity.SubjectCode = model.SubjectCode;
        entity.SubjectName = model.SubjectName;
        entity.Credit = model.Credit;
    }

    public static EnrollmentBusinessModel ToBusinessModel(
        this Enrollment entity,
        bool includeStudent = false,
        bool includeCourse = false,
        bool includeSemester = false) =>
        new()
        {
            EnrollmentId = entity.EnrollmentId,
            StudentId = entity.StudentId,
            CourseId = entity.CourseId,
            EnrollDate = entity.EnrollDate,
            Status = entity.Status,
            Student = includeStudent && entity.Student is not null
                ? new EnrollmentStudentBriefBusinessModel
                {
                    StudentId = entity.Student.StudentId,
                    FullName = entity.Student.FullName,
                    Email = entity.Student.Email
                }
                : null,
            Course = includeCourse && entity.Course is not null
                ? new EnrollmentCourseBriefBusinessModel
                {
                    CourseId = entity.Course.CourseId,
                    CourseName = entity.Course.CourseName,
                    SemesterId = entity.Course.SemesterId,
                    Semester = includeSemester && entity.Course.Semester is not null
                        ? new CourseSemesterBriefBusinessModel
                        {
                            SemesterId = entity.Course.Semester.SemesterId,
                            SemesterName = entity.Course.Semester.SemesterName,
                            StartDate = entity.Course.Semester.StartDate,
                            EndDate = entity.Course.Semester.EndDate
                        }
                        : null
                }
                : null
        };

    public static Enrollment ToEntity(this EnrollmentBusinessModel model) =>
        new()
        {
            StudentId = model.StudentId,
            CourseId = model.CourseId,
            EnrollDate = model.EnrollDate,
            Status = model.Status
        };

    public static void UpdateEntity(this Enrollment entity, EnrollmentBusinessModel model)
    {
        entity.StudentId = model.StudentId;
        entity.CourseId = model.CourseId;
        entity.EnrollDate = model.EnrollDate;
        entity.Status = model.Status;
    }
}
