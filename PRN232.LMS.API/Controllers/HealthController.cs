using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRN232.LMS.API.Common;
using PRN232.LMS.Repositories.Context;

namespace PRN232.LMS.API.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    private readonly LmsDbContext _dbContext;

    public HealthController(LmsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("db")]
    public async Task<ActionResult<ApiResponse<object>>> CheckDatabaseConnection()
    {
        try
        {
            var canConnect = await _dbContext.Database.CanConnectAsync();

            if (!canConnect)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    ApiResponse<object>.Fail("Cannot connect to the local database"));
            }

            return Ok(ApiResponse<object>.Ok(new
            {
                Connected = true,
                Database = _dbContext.Database.GetDbConnection().Database,
                DataSource = _dbContext.Database.GetDbConnection().DataSource
            }, "Connected to the local database successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                ApiResponse<object>.Fail("Database connection check failed", ex.Message));
        }
    }
}
