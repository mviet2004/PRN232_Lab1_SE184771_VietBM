using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.Common;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Mappings;
using PRN232.LMS.Services.Models;
using PRN232.LMS.Services.Models.Queries;

namespace PRN232.LMS.Services.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly IUnitOfWork _unitOfWork;

    public EnrollmentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<EnrollmentBusinessModel>> GetListAsync(EnrollmentListQuery query)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var size = query.Size < 1 ? 10 : query.Size;

        var includeStudent = ShouldExpand(query.Expand, "student");
        var includeCourse = ShouldExpand(query.Expand, "course");

        IQueryable<Enrollment> enrollmentsQuery = (includeStudent, includeCourse) switch
        {
            (true, true) => _unitOfWork.Enrollments.GetAll(null, e => e.Student, e => e.Course),
            (true, false) => _unitOfWork.Enrollments.GetAll(null, e => e.Student),
            (false, true) => _unitOfWork.Enrollments.GetAll(null, e => e.Course),
            _ => _unitOfWork.Enrollments.GetAll()
        };

        if (query.StudentId.HasValue)
        {
            enrollmentsQuery = enrollmentsQuery.Where(e => e.StudentId == query.StudentId.Value);
        }

        if (query.CourseId.HasValue)
        {
            enrollmentsQuery = enrollmentsQuery.Where(e => e.CourseId == query.CourseId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var keyword = query.Search.Trim().ToLowerInvariant();
            enrollmentsQuery = enrollmentsQuery.Where(e =>
                e.Status.ToLower().Contains(keyword) ||
                e.StudentId.ToString().Contains(keyword) ||
                e.CourseId.ToString().Contains(keyword));
        }

        enrollmentsQuery = ApplySorting(enrollmentsQuery, query.Sort);

        var totalItems = await enrollmentsQuery.CountAsync();
        var entities = await enrollmentsQuery
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        return new PagedResult<EnrollmentBusinessModel>
        {
            Items = entities.Select(e => e.ToBusinessModel(includeStudent, includeCourse)).ToList(),
            Page = page,
            PageSize = size,
            TotalItems = totalItems
        };
    }

    public async Task<EnrollmentBusinessModel?> GetByIdAsync(int id, string? expand = null)
    {
        var includeStudent = ShouldExpand(expand, "student");
        var includeCourse = ShouldExpand(expand, "course");

        Enrollment? entity = (includeStudent, includeCourse) switch
        {
            (true, true) => await _unitOfWork.Enrollments.GetByIdAsync(id, e => e.Student, e => e.Course),
            (true, false) => await _unitOfWork.Enrollments.GetByIdAsync(id, e => e.Student),
            (false, true) => await _unitOfWork.Enrollments.GetByIdAsync(id, e => e.Course),
            _ => await _unitOfWork.Enrollments.GetByIdAsync(id)
        };

        return entity?.ToBusinessModel(includeStudent, includeCourse);
    }

    public async Task<EnrollmentBusinessModel> CreateAsync(EnrollmentBusinessModel model)
    {
        var studentExists = await _unitOfWork.Students.ExistsAsync(s => s.StudentId == model.StudentId);
        if (!studentExists)
        {
            throw new InvalidOperationException("Student does not exist.");
        }

        var courseExists = await _unitOfWork.Courses.ExistsAsync(c => c.CourseId == model.CourseId);
        if (!courseExists)
        {
            throw new InvalidOperationException("Course does not exist.");
        }

        var entity = model.ToEntity();
        await _unitOfWork.Enrollments.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        var created = await _unitOfWork.Enrollments.GetByIdAsync(entity.EnrollmentId, e => e.Student, e => e.Course);
        return (created ?? entity).ToBusinessModel(includeStudent: true, includeCourse: true);
    }

    public async Task<EnrollmentBusinessModel?> UpdateAsync(int id, EnrollmentBusinessModel model)
    {
        var entity = await _unitOfWork.Enrollments.GetByIdAsync(id);
        if (entity is null) return null;

        var studentExists = await _unitOfWork.Students.ExistsAsync(s => s.StudentId == model.StudentId);
        if (!studentExists)
        {
            throw new InvalidOperationException("Student does not exist.");
        }

        var courseExists = await _unitOfWork.Courses.ExistsAsync(c => c.CourseId == model.CourseId);
        if (!courseExists)
        {
            throw new InvalidOperationException("Course does not exist.");
        }

        entity.UpdateEntity(model);
        _unitOfWork.Enrollments.Update(entity);
        await _unitOfWork.SaveChangesAsync();

        var updated = await _unitOfWork.Enrollments.GetByIdAsync(id, e => e.Student, e => e.Course);
        return (updated ?? entity).ToBusinessModel(includeStudent: true, includeCourse: true);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _unitOfWork.Enrollments.GetByIdAsync(id);
        if (entity is null) return false;

        _unitOfWork.Enrollments.Remove(entity);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    private static bool ShouldExpand(string? expand, string relation) =>
        !string.IsNullOrWhiteSpace(expand) &&
        expand.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Any(x => x.Equals(relation, StringComparison.OrdinalIgnoreCase));

    private static IQueryable<Enrollment> ApplySorting(IQueryable<Enrollment> query, string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort))
        {
            return query.OrderBy(e => e.EnrollmentId);
        }

        IOrderedQueryable<Enrollment>? ordered = null;
        var fields = sort.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var field in fields)
        {
            var descending = field.StartsWith('-');
            var name = (descending ? field[1..] : field).ToLowerInvariant();

            ordered = (ordered, name) switch
            {
                (null, "enrollmentid") => descending ? query.OrderByDescending(e => e.EnrollmentId) : query.OrderBy(e => e.EnrollmentId),
                (null, "studentid") => descending ? query.OrderByDescending(e => e.StudentId) : query.OrderBy(e => e.StudentId),
                (null, "courseid") => descending ? query.OrderByDescending(e => e.CourseId) : query.OrderBy(e => e.CourseId),
                (null, "enrolldate") => descending ? query.OrderByDescending(e => e.EnrollDate) : query.OrderBy(e => e.EnrollDate),
                (null, "status") => descending ? query.OrderByDescending(e => e.Status) : query.OrderBy(e => e.Status),
                (not null, "enrollmentid") => descending ? ordered.ThenByDescending(e => e.EnrollmentId) : ordered.ThenBy(e => e.EnrollmentId),
                (not null, "studentid") => descending ? ordered.ThenByDescending(e => e.StudentId) : ordered.ThenBy(e => e.StudentId),
                (not null, "courseid") => descending ? ordered.ThenByDescending(e => e.CourseId) : ordered.ThenBy(e => e.CourseId),
                (not null, "enrolldate") => descending ? ordered.ThenByDescending(e => e.EnrollDate) : ordered.ThenBy(e => e.EnrollDate),
                (not null, "status") => descending ? ordered.ThenByDescending(e => e.Status) : ordered.ThenBy(e => e.Status),
                _ => ordered
            };
        }

        return ordered ?? query.OrderBy(e => e.EnrollmentId);
    }
}
