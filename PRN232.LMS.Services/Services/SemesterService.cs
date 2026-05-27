using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.Common;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Mappings;
using PRN232.LMS.Services.Models;
using PRN232.LMS.Services.Models.Queries;

namespace PRN232.LMS.Services.Services;

public class SemesterService : ISemesterService
{
    private readonly IUnitOfWork _unitOfWork;

    public SemesterService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<SemesterBusinessModel>> GetListAsync(SemesterListQuery query)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var size = query.Size < 1 ? 10 : query.Size;
        var includeCourses = ShouldExpand(query.Expand, "courses");

        IQueryable<Semester> semestersQuery = includeCourses
            ? _unitOfWork.Semesters.GetAll(null, s => s.Courses)
            : _unitOfWork.Semesters.GetAll();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var keyword = query.Search.Trim().ToLowerInvariant();
            semestersQuery = semestersQuery.Where(s => s.SemesterName.ToLower().Contains(keyword));
        }

        semestersQuery = ApplySorting(semestersQuery, query.Sort);

        var totalItems = await semestersQuery.CountAsync();
        var entities = await semestersQuery
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        return new PagedResult<SemesterBusinessModel>
        {
            Items = entities.Select(e => e.ToBusinessModel(includeCourses)).ToList(),
            Page = page,
            PageSize = size,
            TotalItems = totalItems
        };
    }

    public async Task<SemesterBusinessModel?> GetByIdAsync(int id)
    {
        var entity = await _unitOfWork.Semesters.GetByIdAsync(id);
        return entity?.ToBusinessModel();
    }

    public async Task<SemesterBusinessModel> CreateAsync(SemesterBusinessModel model)
    {
        var entity = model.ToEntity();
        await _unitOfWork.Semesters.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();
        return entity.ToBusinessModel();
    }

    public async Task<SemesterBusinessModel?> UpdateAsync(int id, SemesterBusinessModel model)
    {
        var entity = await _unitOfWork.Semesters.GetByIdAsync(id);
        if (entity is null) return null;

        entity.UpdateEntity(model);
        _unitOfWork.Semesters.Update(entity);
        await _unitOfWork.SaveChangesAsync();
        return entity.ToBusinessModel();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _unitOfWork.Semesters.GetByIdAsync(id);
        if (entity is null) return false;

        _unitOfWork.Semesters.Remove(entity);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    private static bool ShouldExpand(string? expand, string relation) =>
        !string.IsNullOrWhiteSpace(expand) &&
        expand.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Any(x => x.Equals(relation, StringComparison.OrdinalIgnoreCase));

    private static IQueryable<Semester> ApplySorting(IQueryable<Semester> query, string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort))
        {
            return query.OrderBy(s => s.SemesterId);
        }

        IOrderedQueryable<Semester>? ordered = null;
        var fields = sort.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var field in fields)
        {
            var descending = field.StartsWith('-');
            var name = (descending ? field[1..] : field).ToLowerInvariant();

            ordered = (ordered, name) switch
            {
                (null, "semesterid") => descending ? query.OrderByDescending(s => s.SemesterId) : query.OrderBy(s => s.SemesterId),
                (null, "semestername") => descending ? query.OrderByDescending(s => s.SemesterName) : query.OrderBy(s => s.SemesterName),
                (null, "startdate") => descending ? query.OrderByDescending(s => s.StartDate) : query.OrderBy(s => s.StartDate),
                (null, "enddate") => descending ? query.OrderByDescending(s => s.EndDate) : query.OrderBy(s => s.EndDate),
                (not null, "semesterid") => descending ? ordered.ThenByDescending(s => s.SemesterId) : ordered.ThenBy(s => s.SemesterId),
                (not null, "semestername") => descending ? ordered.ThenByDescending(s => s.SemesterName) : ordered.ThenBy(s => s.SemesterName),
                (not null, "startdate") => descending ? ordered.ThenByDescending(s => s.StartDate) : ordered.ThenBy(s => s.StartDate),
                (not null, "enddate") => descending ? ordered.ThenByDescending(s => s.EndDate) : ordered.ThenBy(s => s.EndDate),
                _ => ordered
            };
        }

        return ordered ?? query.OrderBy(s => s.SemesterId);
    }
}
