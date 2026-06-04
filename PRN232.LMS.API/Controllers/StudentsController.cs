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
    private static readonly HashSet<string> AllowedFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "studentId",
        "fullName",
        "email",
        "dateOfBirth",
        "enrollments"
    };

    private static readonly HashSet<string> AllowedSortFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "studentId",
        "fullName",
        "email",
        "dateOfBirth"
    };

    private static readonly HashSet<string> AllowedExpansions = new(StringComparer.OrdinalIgnoreCase)
    {
        "enrollments"
    };

    private readonly IStudentService _studentService;

    public StudentsController(IStudentService studentService)
    {
        _studentService = studentService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedData<object>>>> GetList(
        [FromQuery] string? search,
        [FromQuery] string? sort,
        [FromQuery] int? page,
        [FromQuery] int? size,
        [FromQuery] string? fields = null,
        [FromQuery] string? expand = null)
    {
        var errors = new Dictionary<string, string>();

        if (page.HasValue && page.Value < 1)
        {
            errors["page"] = "Page must be greater than or equal to 1.";
        }

        if (size.HasValue && size.Value < 1)
        {
            errors["size"] = "Size must be greater than or equal to 1.";
        }

        var invalidSortFields = GetInvalidSortFields(sort);
        if (invalidSortFields.Count > 0)
        {
            errors["sort"] = $"Unsupported sort field(s): {string.Join(", ", invalidSortFields)}.";
        }

        var invalidFields = GetInvalidListValues(fields, AllowedFields);
        if (invalidFields.Count > 0)
        {
            errors["fields"] = $"Unsupported field(s): {string.Join(", ", invalidFields)}.";
        }

        var invalidExpansions = GetInvalidListValues(expand, AllowedExpansions);
        if (invalidExpansions.Count > 0)
        {
            errors["expand"] = $"Unsupported expansion(s): {string.Join(", ", invalidExpansions)}.";
        }

        if (errors.Count > 0)
        {
            return BadRequest(ApiResponse<PagedData<object>>.Fail("Missing or invalid query parameters", errors));
        }

        var pageValue = page.GetValueOrDefault(1);
        var sizeValue = size.GetValueOrDefault(10);

        var result = await _studentService.GetListAsync(new StudentListQuery
        {
            Search = search,
            Sort = sort,
            Page = pageValue,
            Size = sizeValue,
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

    private static List<string> GetInvalidSortFields(string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort))
        {
            return new List<string>();
        }

        return sort.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(field => field.StartsWith('-') ? field[1..] : field)
            .Where(field => !AllowedSortFields.Contains(field))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static List<string> GetInvalidListValues(string? values, HashSet<string> allowedValues)
    {
        if (string.IsNullOrWhiteSpace(values))
        {
            return new List<string>();
        }

        return values.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(value => !allowedValues.Contains(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
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
