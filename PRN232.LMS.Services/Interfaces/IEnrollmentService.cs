using PRN232.LMS.Services.Common;
using PRN232.LMS.Services.Models;
using PRN232.LMS.Services.Models.Queries;

namespace PRN232.LMS.Services.Interfaces;

public interface IEnrollmentService
{
    Task<PagedResult<EnrollmentBusinessModel>> GetListAsync(EnrollmentListQuery query);
    Task<EnrollmentBusinessModel?> GetByIdAsync(int id, string? expand = null);
    Task<EnrollmentBusinessModel> CreateAsync(EnrollmentBusinessModel model);
    Task<EnrollmentBusinessModel?> UpdateAsync(int id, EnrollmentBusinessModel model);
    Task<bool> DeleteAsync(int id);
}
