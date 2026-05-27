using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.Common;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Mappings;
using PRN232.LMS.Services.Models;
using PRN232.LMS.Services.Models.Queries;

namespace PRN232.LMS.Services.Services;

public class SubjectService : ISubjectService
{
    private readonly IUnitOfWork _unitOfWork;

    public SubjectService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<SubjectBusinessModel>> GetListAsync(SubjectListQuery query)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var size = query.Size < 1 ? 10 : query.Size;
        IQueryable<Subject> subjectsQuery = _unitOfWork.Subjects.GetAll();

        if (query.Credit.HasValue)
        {
            subjectsQuery = subjectsQuery.Where(s => s.Credit == query.Credit.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var keyword = query.Search.Trim().ToLowerInvariant();
            subjectsQuery = subjectsQuery.Where(s =>
                s.SubjectCode.ToLower().Contains(keyword) ||
                s.SubjectName.ToLower().Contains(keyword) ||
                s.Credit.ToString().Contains(keyword));
        }

        subjectsQuery = ApplySorting(subjectsQuery, query.Sort);

        var totalItems = await subjectsQuery.CountAsync();
        var entities = await subjectsQuery
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        return new PagedResult<SubjectBusinessModel>
        {
            Items = entities.Select(e => e.ToBusinessModel()).ToList(),
            Page = page,
            PageSize = size,
            TotalItems = totalItems
        };
    }

    public async Task<SubjectBusinessModel?> GetByIdAsync(int id)
    {
        var entity = await _unitOfWork.Subjects.GetByIdAsync(id);
        return entity?.ToBusinessModel();
    }

    public async Task<SubjectBusinessModel> CreateAsync(SubjectBusinessModel model)
    {
        var entity = model.ToEntity();
        await _unitOfWork.Subjects.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();
        return entity.ToBusinessModel();
    }

    public async Task<SubjectBusinessModel?> UpdateAsync(int id, SubjectBusinessModel model)
    {
        var entity = await _unitOfWork.Subjects.GetByIdAsync(id);
        if (entity is null) return null;

        entity.UpdateEntity(model);
        _unitOfWork.Subjects.Update(entity);
        await _unitOfWork.SaveChangesAsync();
        return entity.ToBusinessModel();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _unitOfWork.Subjects.GetByIdAsync(id);
        if (entity is null) return false;

        _unitOfWork.Subjects.Remove(entity);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    private static IQueryable<Subject> ApplySorting(IQueryable<Subject> query, string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort))
        {
            return query.OrderBy(s => s.SubjectId);
        }

        IOrderedQueryable<Subject>? ordered = null;
        var fields = sort.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var field in fields)
        {
            var descending = field.StartsWith('-');
            var name = (descending ? field[1..] : field).ToLowerInvariant();

            ordered = (ordered, name) switch
            {
                (null, "subjectid") => descending ? query.OrderByDescending(s => s.SubjectId) : query.OrderBy(s => s.SubjectId),
                (null, "subjectcode") => descending ? query.OrderByDescending(s => s.SubjectCode) : query.OrderBy(s => s.SubjectCode),
                (null, "subjectname") => descending ? query.OrderByDescending(s => s.SubjectName) : query.OrderBy(s => s.SubjectName),
                (null, "credit") => descending ? query.OrderByDescending(s => s.Credit) : query.OrderBy(s => s.Credit),
                (not null, "subjectid") => descending ? ordered.ThenByDescending(s => s.SubjectId) : ordered.ThenBy(s => s.SubjectId),
                (not null, "subjectcode") => descending ? ordered.ThenByDescending(s => s.SubjectCode) : ordered.ThenBy(s => s.SubjectCode),
                (not null, "subjectname") => descending ? ordered.ThenByDescending(s => s.SubjectName) : ordered.ThenBy(s => s.SubjectName),
                (not null, "credit") => descending ? ordered.ThenByDescending(s => s.Credit) : ordered.ThenBy(s => s.Credit),
                _ => ordered
            };
        }

        return ordered ?? query.OrderBy(s => s.SubjectId);
    }
}
