using PRN232.LMS.Services.Common;
using PRN232.LMS.Services.Models;
using PRN232.LMS.Services.Models.Queries;

namespace PRN232.LMS.Services.Interfaces;

public interface IStudentService
{
    Task<StudentBusinessModel?> GetByIdAsync(int id, string? expand = null);
    Task<PagedResult<StudentBusinessModel>> GetListAsync(StudentListQuery query);
    Task<StudentBusinessModel> CreateAsync(StudentBusinessModel model);
    Task<StudentBusinessModel?> UpdateAsync(int id, StudentBusinessModel model);
    Task<bool> DeleteAsync(int id);
}
