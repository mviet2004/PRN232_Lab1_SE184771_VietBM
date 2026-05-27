using Microsoft.Extensions.DependencyInjection;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Services;

namespace PRN232.LMS.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddServiceLayer(this IServiceCollection services)
    {
        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<ISemesterService, SemesterService>();
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<ISubjectService, SubjectService>();
        services.AddScoped<IEnrollmentService, EnrollmentService>();
        return services;
    }
}
