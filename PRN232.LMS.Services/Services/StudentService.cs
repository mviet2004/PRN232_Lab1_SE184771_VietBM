using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.Common;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Mappings;
using PRN232.LMS.Services.Models;
using PRN232.LMS.Services.Models.Queries;

namespace PRN232.LMS.Services.Services;

public class StudentService : IStudentService
{
    private readonly IUnitOfWork _unitOfWork;

    public StudentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<StudentBusinessModel?> GetByIdAsync(int id, string? expand = null)
    {
        var includeEnrollments = ShouldExpand(expand, "enrollments");
        Student? entity;

        if (includeEnrollments)
        {
            entity = await _unitOfWork.Students.GetByIdAsync(id, s => s.Enrollments);
        }
        else
        {
            entity = await _unitOfWork.Students.GetByIdAsync(id);
        }

        return entity?.ToBusinessModel(includeEnrollments);
    }

    public async Task<PagedResult<StudentBusinessModel>> GetListAsync(StudentListQuery query)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var size = query.Size < 1 ? 10 : query.Size;
        var includeEnrollments = ShouldExpand(query.Expand, "enrollments");

        IQueryable<Student> studentsQuery = includeEnrollments
            ? _unitOfWork.Students.GetAll(null, s => s.Enrollments)
            : _unitOfWork.Students.GetAll();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var keyword = query.Search.Trim().ToLower();
            studentsQuery = studentsQuery.Where(s =>
                s.FullName.ToLower().Contains(keyword) ||
                s.Email.ToLower().Contains(keyword));
        }

        studentsQuery = ApplySorting(studentsQuery, query.Sort);

        var totalItems = await studentsQuery.CountAsync();
        var entities = await studentsQuery
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        return new PagedResult<StudentBusinessModel>
        {
            Items = entities.Select(e => e.ToBusinessModel(includeEnrollments)).ToList(),
            Page = page,
            PageSize = size,
            TotalItems = totalItems
        };
    }

    public async Task<StudentBusinessModel> CreateAsync(StudentBusinessModel model)
    {
        var entity = model.ToEntity();
        await _unitOfWork.Students.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();
        return entity.ToBusinessModel();
    }

    public async Task<StudentBusinessModel?> UpdateAsync(int id, StudentBusinessModel model)
    {
        var entity = await _unitOfWork.Students.GetByIdAsync(id);
        if (entity is null)
        {
            return null;
        }

        entity.UpdateEntity(model);
        _unitOfWork.Students.Update(entity);
        await _unitOfWork.SaveChangesAsync();
        return entity.ToBusinessModel();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _unitOfWork.Students.GetByIdAsync(id);
        if (entity is null)
        {
            return false;
        }

        _unitOfWork.Students.Remove(entity);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    private static bool ShouldExpand(string? expand, string relation) =>
        !string.IsNullOrWhiteSpace(expand) &&
        expand.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Any(x => x.Equals(relation, StringComparison.OrdinalIgnoreCase));

    private static IQueryable<Student> ApplySorting(IQueryable<Student> query, string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort))
        {
            return query.OrderBy(s => s.StudentId);
        }

        IOrderedQueryable<Student>? ordered = null;
        var fields = sort.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var field in fields)
        {
            var descending = field.StartsWith('-');
            var name = (descending ? field[1..] : field).ToLowerInvariant();

            ordered = (ordered, name) switch
            {
                (null, "fullname") => descending ? query.OrderByDescending(s => s.FullName) : query.OrderBy(s => s.FullName),
                (null, "email") => descending ? query.OrderByDescending(s => s.Email) : query.OrderBy(s => s.Email),
                (null, "dateofbirth") => descending ? query.OrderByDescending(s => s.DateOfBirth) : query.OrderBy(s => s.DateOfBirth),
                (null, "studentid") => descending ? query.OrderByDescending(s => s.StudentId) : query.OrderBy(s => s.StudentId),
                (not null, "fullname") => descending ? ordered.ThenByDescending(s => s.FullName) : ordered.ThenBy(s => s.FullName),
                (not null, "email") => descending ? ordered.ThenByDescending(s => s.Email) : ordered.ThenBy(s => s.Email),
                (not null, "dateofbirth") => descending ? ordered.ThenByDescending(s => s.DateOfBirth) : ordered.ThenBy(s => s.DateOfBirth),
                (not null, "studentid") => descending ? ordered.ThenByDescending(s => s.StudentId) : ordered.ThenBy(s => s.StudentId),
                _ => ordered
            };
        }

        return ordered ?? query.OrderBy(s => s.StudentId);
    }
}
