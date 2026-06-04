using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.Common;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Mappings;
using PRN232.LMS.Services.Models;
using PRN232.LMS.Services.Models.Queries;

namespace PRN232.LMS.Services.Services;

public class CourseService : ICourseService
{
    private readonly IUnitOfWork _unitOfWork;

    public CourseService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<CourseBusinessModel>> GetListAsync(CourseListQuery query)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var size = query.Size < 1 ? 10 : query.Size;
        var includeSemester = ShouldExpand(query.Expand, "semester");
        var includeEnrollments = ShouldExpand(query.Expand, "enrollments");

        IQueryable<Course> coursesQuery = (includeSemester, includeEnrollments) switch
        {
            (true, true) => _unitOfWork.Courses.GetAll(null, c => c.Semester, c => c.Enrollments),
            (true, false) => _unitOfWork.Courses.GetAll(null, c => c.Semester),
            (false, true) => _unitOfWork.Courses.GetAll(null, c => c.Enrollments),
            _ => _unitOfWork.Courses.GetAll()
        };

        if (query.SemesterId.HasValue)
        {
            coursesQuery = coursesQuery.Where(c => c.SemesterId == query.SemesterId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var keyword = query.Search.Trim().ToLowerInvariant();
            coursesQuery = coursesQuery.Where(c =>
                c.CourseName.ToLower().Contains(keyword) ||
                c.CourseId.ToString().Contains(keyword) ||
                c.SemesterId.ToString().Contains(keyword));
        }

        coursesQuery = ApplySorting(coursesQuery, query.Sort);

        var totalItems = await coursesQuery.CountAsync();
        var entities = await coursesQuery
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        return new PagedResult<CourseBusinessModel>
        {
            Items = entities.Select(e => e.ToBusinessModel(includeSemester, includeEnrollments)).ToList(),
            Page = page,
            PageSize = size,
            TotalItems = totalItems
        };
    }

    public async Task<CourseBusinessModel?> GetByIdAsync(int id)
    {
        var entity = await _unitOfWork.Courses.GetAll()
            .Include(c => c.Semester)
            .Include(c => c.Enrollments)
            .ThenInclude(e => e.Student)
            .FirstOrDefaultAsync(c => c.CourseId == id);

        return entity?.ToBusinessModel(includeSemester: true, includeEnrollments: true);
    }

    public async Task<CourseBusinessModel> CreateAsync(CourseBusinessModel model)
    {
        var semesterExists = await _unitOfWork.Semesters.ExistsAsync(s => s.SemesterId == model.SemesterId);
        if (!semesterExists)
        {
            throw new InvalidOperationException("Semester does not exist.");
        }

        var entity = model.ToEntity();
        await _unitOfWork.Courses.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();
        return entity.ToBusinessModel();
    }

    public async Task<CourseBusinessModel?> UpdateAsync(int id, CourseBusinessModel model)
    {
        var entity = await _unitOfWork.Courses.GetByIdAsync(id);
        if (entity is null) return null;

        var semesterExists = await _unitOfWork.Semesters.ExistsAsync(s => s.SemesterId == model.SemesterId);
        if (!semesterExists)
        {
            throw new InvalidOperationException("Semester does not exist.");
        }

        entity.UpdateEntity(model);
        _unitOfWork.Courses.Update(entity);
        await _unitOfWork.SaveChangesAsync();
        return entity.ToBusinessModel();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _unitOfWork.Courses.GetByIdAsync(id);
        if (entity is null) return false;

        _unitOfWork.Courses.Remove(entity);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    private static bool ShouldExpand(string? expand, string relation) =>
        !string.IsNullOrWhiteSpace(expand) &&
        expand.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Any(x => x.Equals(relation, StringComparison.OrdinalIgnoreCase));

    private static IQueryable<Course> ApplySorting(IQueryable<Course> query, string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort))
        {
            return query.OrderBy(c => c.CourseId);
        }

        IOrderedQueryable<Course>? ordered = null;
        var fields = sort.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var field in fields)
        {
            var descending = field.StartsWith('-');
            var name = (descending ? field[1..] : field).ToLowerInvariant();

            ordered = (ordered, name) switch
            {
                (null, "courseid") => descending ? query.OrderByDescending(c => c.CourseId) : query.OrderBy(c => c.CourseId),
                (null, "coursename") => descending ? query.OrderByDescending(c => c.CourseName) : query.OrderBy(c => c.CourseName),
                (null, "semesterid") => descending ? query.OrderByDescending(c => c.SemesterId) : query.OrderBy(c => c.SemesterId),
                (not null, "courseid") => descending ? ordered.ThenByDescending(c => c.CourseId) : ordered.ThenBy(c => c.CourseId),
                (not null, "coursename") => descending ? ordered.ThenByDescending(c => c.CourseName) : ordered.ThenBy(c => c.CourseName),
                (not null, "semesterid") => descending ? ordered.ThenByDescending(c => c.SemesterId) : ordered.ThenBy(c => c.SemesterId),
                _ => ordered
            };
        }

        return ordered ?? query.OrderBy(c => c.CourseId);
    }
}
