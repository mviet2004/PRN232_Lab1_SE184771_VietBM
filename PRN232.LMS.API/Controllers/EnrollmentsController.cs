using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Common;
using PRN232.LMS.API.Mappings;
using PRN232.LMS.API.Models.Requests;
using PRN232.LMS.API.Models.Responses;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models.Queries;

namespace PRN232.LMS.API.Controllers;

[ApiController]
[Route("api/enrollments")]
public class EnrollmentsController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;

    public EnrollmentsController(IEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedData<object>>>> GetList(
        [FromQuery] string? search,
        [FromQuery] string? sort,
        [FromQuery] int page = 1,
        [FromQuery] int size = 10,
        [FromQuery] string? fields = null,
        [FromQuery] string? expand = null,
        [FromQuery] int? studentId = null,
        [FromQuery] int? courseId = null)
    {
        var result = await _enrollmentService.GetListAsync(new EnrollmentListQuery
        {
            Search = search,
            Sort = sort,
            Page = page,
            Size = size,
            Fields = fields,
            Expand = expand,
            StudentId = studentId,
            CourseId = courseId
        });

        var items = result.Items.Select(x => x.ToResponseObject(fields)).ToList();

        var data = new PagedData<object>
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

        return Ok(ApiResponse<PagedData<object>>.Ok(data));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<EnrollmentResponse>>> GetById(int id, [FromQuery] string? expand = null)
    {
        var item = await _enrollmentService.GetByIdAsync(id, expand);
        if (item is null)
        {
            return NotFound(ApiResponse<EnrollmentResponse>.Fail("Enrollment not found"));
        }

        return Ok(ApiResponse<EnrollmentResponse>.Ok(item.ToResponse()));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<EnrollmentResponse>>> Create([FromBody] CreateEnrollmentRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<EnrollmentResponse>.Fail("Invalid request", ModelState));
        }

        try
        {
            var created = await _enrollmentService.CreateAsync(request.ToBusinessModel());
            return CreatedAtAction(nameof(GetById), new { id = created.EnrollmentId },
                ApiResponse<EnrollmentResponse>.Ok(created.ToResponse(), "Enrollment created successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<EnrollmentResponse>.Fail(ex.Message));
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<EnrollmentResponse>>> Update(int id, [FromBody] UpdateEnrollmentRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<EnrollmentResponse>.Fail("Invalid request", ModelState));
        }

        try
        {
            var updated = await _enrollmentService.UpdateAsync(id, request.ToBusinessModel());
            if (updated is null)
            {
                return NotFound(ApiResponse<EnrollmentResponse>.Fail("Enrollment not found"));
            }

            return Ok(ApiResponse<EnrollmentResponse>.Ok(updated.ToResponse(), "Enrollment updated successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<EnrollmentResponse>.Fail(ex.Message));
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        var deleted = await _enrollmentService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound(ApiResponse<object>.Fail("Enrollment not found"));
        }

        return Ok(ApiResponse<object>.Ok(new { }, "Enrollment deleted successfully"));
    }
}
