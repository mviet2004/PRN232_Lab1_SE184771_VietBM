using PRN232.LMS.Services.Models;
using PRN232.LMS.Services.Common;
using PRN232.LMS.Services.Models.Queries;

namespace PRN232.LMS.Services.Interfaces;

public interface ISemesterService
{
    Task<PagedResult<SemesterBusinessModel>> GetListAsync(SemesterListQuery query);
    Task<SemesterBusinessModel?> GetByIdAsync(int id);
    Task<SemesterBusinessModel> CreateAsync(SemesterBusinessModel model);
    Task<SemesterBusinessModel?> UpdateAsync(int id, SemesterBusinessModel model);
    Task<bool> DeleteAsync(int id);
}
