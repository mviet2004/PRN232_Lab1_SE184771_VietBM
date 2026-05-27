using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Common;
using PRN232.LMS.API.Mappings;
using PRN232.LMS.API.Models.Requests;
using PRN232.LMS.API.Models.Responses;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models.Queries;

namespace PRN232.LMS.API.Controllers;

[ApiController]
[Route("api/students")]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _studentService;

    public StudentsController(IStudentService studentService)
    {
        _studentService = studentService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedData<object>>>> GetList(
        [FromQuery] string? search,
        [FromQuery] string? sort,
        [FromQuery] int page = 1,
        [FromQuery] int size = 10,
        [FromQuery] string? fields = null,
        [FromQuery] string? expand = null)
    {
        var result = await _studentService.GetListAsync(new StudentListQuery
        {
            Search = search,
            Sort = sort,
            Page = page,
            Size = size,
            Fields = fields,
            Expand = expand
        });

        var items = result.Items
            .Select(s => s.ToResponseObject(fields))
            .ToList();

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
    public async Task<ActionResult<ApiResponse<StudentResponse>>> GetById(
        int id,
        [FromQuery] string? expand = null)
    {
        var student = await _studentService.GetByIdAsync(id, expand);
        if (student is null)
        {
            return NotFound(ApiResponse<StudentResponse>.Fail("Student not found"));
        }

        return Ok(ApiResponse<StudentResponse>.Ok(student.ToResponse()));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<StudentResponse>>> Create([FromBody] CreateStudentRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<StudentResponse>.Fail("Invalid request", ModelState));
        }

        var created = await _studentService.CreateAsync(request.ToBusinessModel());
        return CreatedAtAction(
            nameof(GetById),
            new { id = created.StudentId },
            ApiResponse<StudentResponse>.Ok(created.ToResponse(), "Student created successfully"));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<StudentResponse>>> Update(
        int id,
        [FromBody] UpdateStudentRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<StudentResponse>.Fail("Invalid request", ModelState));
        }

        var updated = await _studentService.UpdateAsync(id, request.ToBusinessModel());
        if (updated is null)
        {
            return NotFound(ApiResponse<StudentResponse>.Fail("Student not found"));
        }

        return Ok(ApiResponse<StudentResponse>.Ok(updated.ToResponse(), "Student updated successfully"));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        var deleted = await _studentService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound(ApiResponse<object>.Fail("Student not found"));
        }

        return Ok(ApiResponse<object>.Ok(new { }, "Student deleted successfully"));
    }
}
