using PRN232.LMS.Services.Models;
using PRN232.LMS.Services.Common;
using PRN232.LMS.Services.Models.Queries;

namespace PRN232.LMS.Services.Interfaces;

public interface ICourseService
{
    Task<PagedResult<CourseBusinessModel>> GetListAsync(CourseListQuery query);
    Task<CourseBusinessModel?> GetByIdAsync(int id);
    Task<CourseBusinessModel> CreateAsync(CourseBusinessModel model);
    Task<CourseBusinessModel?> UpdateAsync(int id, CourseBusinessModel model);
    Task<bool> DeleteAsync(int id);
}
