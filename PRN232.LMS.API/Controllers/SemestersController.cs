using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Common;
using PRN232.LMS.API.Mappings;
using PRN232.LMS.API.Models.Requests;
using PRN232.LMS.API.Models.Responses;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models.Queries;

namespace PRN232.LMS.API.Controllers;

[ApiController]
[Route("api/semesters")]
public class SemestersController : ControllerBase
{
    private readonly ISemesterService _semesterService;

    public SemestersController(ISemesterService semesterService)
    {
        _semesterService = semesterService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedData<SemesterResponse>>>> GetList(
        [FromQuery] string? search,
        [FromQuery] string? sort,
        [FromQuery] int page = 1,
        [FromQuery] int size = 10,
        [FromQuery] string? fields = null,
        [FromQuery] string? expand = null)
    {
        var result = await _semesterService.GetListAsync(new SemesterListQuery
        {
            Search = search,
            Sort = sort,
            Page = page,
            Size = size,
            Fields = fields,
            Expand = expand
        });

        var items = result.Items.Select(x => x.ToResponse()).ToList();

        var data = new PagedData<SemesterResponse>
        {
            Items = items,
            Pagination = new PaginationMetadata
            {
                Page = result.Page,
                PageSize = result.PageSize,
                TotalItems = result.TotalItems,
                TotalPages = result.TotalPages
            }
        };

        return Ok(ApiResponse<PagedData<SemesterResponse>>.Ok(data));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<SemesterResponse>>> GetById(int id)
    {
        var item = await _semesterService.GetByIdAsync(id);
        if (item is null)
        {
            return NotFound(ApiResponse<SemesterResponse>.Fail("Semester not found"));
        }

        return Ok(ApiResponse<SemesterResponse>.Ok(item.ToResponse()));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<SemesterResponse>>> Create([FromBody] CreateSemesterRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<SemesterResponse>.ValidationFail(ModelState));
        }

        var created = await _semesterService.CreateAsync(request.ToBusinessModel());
        return CreatedAtAction(nameof(GetById), new { id = created.SemesterId },
            ApiResponse<SemesterResponse>.Ok(created.ToResponse(), "Semester created successfully"));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<SemesterResponse>>> Update(int id, [FromBody] UpdateSemesterRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<SemesterResponse>.ValidationFail(ModelState));
        }

        var updated = await _semesterService.UpdateAsync(id, request.ToBusinessModel());
        if (updated is null)
        {
            return NotFound(ApiResponse<SemesterResponse>.Fail("Semester not found"));
        }

        return Ok(ApiResponse<SemesterResponse>.Ok(updated.ToResponse(), "Semester updated successfully"));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<EmptyResponse>>> Delete(int id)
    {
        var deleted = await _semesterService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound(ApiResponse<EmptyResponse>.Fail("Semester not found"));
        }

        return Ok(ApiResponse<EmptyResponse>.Ok(new EmptyResponse(), "Semester deleted successfully"));
    }
}
