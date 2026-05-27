using PRN232.LMS.Services.Models;
using PRN232.LMS.Services.Common;
using PRN232.LMS.Services.Models.Queries;

namespace PRN232.LMS.Services.Interfaces;

public interface ISubjectService
{
    Task<PagedResult<SubjectBusinessModel>> GetListAsync(SubjectListQuery query);
    Task<SubjectBusinessModel?> GetByIdAsync(int id);
    Task<SubjectBusinessModel> CreateAsync(SubjectBusinessModel model);
    Task<SubjectBusinessModel?> UpdateAsync(int id, SubjectBusinessModel model);
    Task<bool> DeleteAsync(int id);
}
