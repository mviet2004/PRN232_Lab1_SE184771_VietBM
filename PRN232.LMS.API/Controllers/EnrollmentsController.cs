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
    private static readonly HashSet<string> AllowedFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "enrollmentId",
        "studentId",
        "courseId",
        "enrollDate",
        "status",
        "student",
        "course"
    };

    private static readonly HashSet<string> AllowedSortFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "enrollmentId",
        "studentId",
        "courseId",
        "enrollDate",
        "status"
    };

    private static readonly HashSet<string> AllowedExpansions = new(StringComparer.OrdinalIgnoreCase)
    {
        "student",
        "course",
        "semester",
        "course.semester",
        "all"
    };

    private readonly IEnrollmentService _enrollmentService;

    public EnrollmentsController(IEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedData<object>>>> GetList(
        [FromQuery] string? search,
        [FromQuery] string? sort,
        [FromQuery] int? page,
        [FromQuery] int? size,
        [FromQuery] string? fields = null,
        [FromQuery] string? expand = null,
        [FromQuery] int? studentId = null,
        [FromQuery] int? courseId = null)
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

        if (studentId.HasValue && studentId.Value < 1)
        {
            errors["studentId"] = "StudentId must be greater than or equal to 1.";
        }

        if (courseId.HasValue && courseId.Value < 1)
        {
            errors["courseId"] = "CourseId must be greater than or equal to 1.";
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

        var result = await _enrollmentService.GetListAsync(new EnrollmentListQuery
        {
            Search = search,
            Sort = sort,
            Page = pageValue,
            Size = sizeValue,
            Fields = fields,
            Expand = expand,
            StudentId = studentId,
            CourseId = courseId
        });

        var responseFields = MergeFieldsWithExpansions(fields, expand);
        var items = result.Items.Select(x => x.ToResponseObject(responseFields)).ToList();

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

    private static string? MergeFieldsWithExpansions(string? fields, string? expand)
    {
        if (string.IsNullOrWhiteSpace(fields) || string.IsNullOrWhiteSpace(expand))
        {
            return fields;
        }

        var selectedFields = fields
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var expansions = expand
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (expansions.Contains("all") || expansions.Contains("student"))
        {
            selectedFields.Add("student");
        }

        if (expansions.Contains("all") ||
            expansions.Contains("course") ||
            expansions.Contains("semester") ||
            expansions.Contains("course.semester"))
        {
            selectedFields.Add("course");
        }

        return string.Join(",", selectedFields);
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
