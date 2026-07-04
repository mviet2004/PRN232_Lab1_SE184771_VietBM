using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Common;
using PRN232.LMS.API.Mappings;
using PRN232.LMS.API.Models.Requests;
using PRN232.LMS.API.Models.Responses;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models.Queries;

namespace PRN232.LMS.API.Controllers;

[ApiController]
[Route("api/courses")]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _courseService;

    public CoursesController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedData<CourseResponse>>>> GetList(
        [FromQuery] string? search,
        [FromQuery] string? sort,
        [FromQuery] int page = 1,
        [FromQuery] int size = 10,
        [FromQuery] string? fields = null,
        [FromQuery] string? expand = null)
    {
        var result = await _courseService.GetListAsync(new CourseListQuery
        {
            Search = search,
            Sort = sort,
            Page = page,
            Size = size,
            Fields = fields,
            Expand = expand
        });

        var items = result.Items.Select(x => x.ToResponse()).ToList();

        var data = new PagedData<CourseResponse>
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

        return Ok(ApiResponse<PagedData<CourseResponse>>.Ok(data));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<CourseResponse>>> GetById(int id)
    {
        var item = await _courseService.GetByIdAsync(id);
        if (item is null)
        {
            return NotFound(ApiResponse<CourseResponse>.Fail("Course not found"));
        }

        return Ok(ApiResponse<CourseResponse>.Ok(item.ToResponse()));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CourseResponse>>> Create([FromBody] CreateCourseRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<CourseResponse>.ValidationFail(ModelState));
        }

        try
        {
            var created = await _courseService.CreateAsync(request.ToBusinessModel());
            return CreatedAtAction(nameof(GetById), new { id = created.CourseId },
                ApiResponse<CourseResponse>.Ok(created.ToResponse(), "Course created successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<CourseResponse>.Fail(ex.Message));
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<CourseResponse>>> Update(int id, [FromBody] UpdateCourseRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<CourseResponse>.ValidationFail(ModelState));
        }

        try
        {
            var updated = await _courseService.UpdateAsync(id, request.ToBusinessModel());
            if (updated is null)
            {
                return NotFound(ApiResponse<CourseResponse>.Fail("Course not found"));
            }

            return Ok(ApiResponse<CourseResponse>.Ok(updated.ToResponse(), "Course updated successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<CourseResponse>.Fail(ex.Message));
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<EmptyResponse>>> Delete(int id)
    {
        var deleted = await _courseService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound(ApiResponse<EmptyResponse>.Fail("Course not found"));
        }

        return Ok(ApiResponse<EmptyResponse>.Ok(new EmptyResponse(), "Course deleted successfully"));
    }
}
