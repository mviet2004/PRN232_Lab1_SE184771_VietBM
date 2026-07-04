using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using PRN232.LMS.API.Common;
using PRN232.LMS.Repositories;
using PRN232.LMS.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRepositoryLayer(builder.Configuration);
builder.Services.AddServiceLayer();
builder.Services.AddControllers(options =>
    {
        options.ReturnHttpNotAcceptable = true;
        options.OutputFormatters.RemoveType<StringOutputFormatter>();
        options.Filters.Add(new ProducesAttribute("application/json", "application/xml"));
    })
    .AddXmlSerializerFormatters()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
            new BadRequestObjectResult(ApiResponse<object>.ValidationFail(context.ModelState));
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception)
    {
        if (!context.Request.Path.StartsWithSegments("/api") || context.Response.HasStarted)
        {
            throw;
        }

        context.Response.Clear();
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        await WriteApiResponseAsync(
            context,
            StatusCodes.Status500InternalServerError,
            ApiResponse<object>.Fail("Internal server error"));
    }
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseStatusCodePages(async statusCodeContext =>
{
    var httpContext = statusCodeContext.HttpContext;

    if (!httpContext.Request.Path.StartsWithSegments("/api") || httpContext.Response.HasStarted)
    {
        return;
    }

    var message = httpContext.Response.StatusCode switch
    {
        StatusCodes.Status404NotFound => "Resource not found",
        StatusCodes.Status405MethodNotAllowed => "Method not allowed",
        _ => "Request failed"
    };

    await WriteApiResponseAsync(
        httpContext,
        httpContext.Response.StatusCode,
        ApiResponse<object>.Fail(message));
});

app.UseAuthorization();

app.MapControllers();

app.Run();

static Task WriteApiResponseAsync(HttpContext httpContext, int statusCode, ApiResponse<object> response)
{
    var result = new ObjectResult(response)
    {
        StatusCode = statusCode
    };

    var actionContext = new ActionContext(
        httpContext,
        httpContext.GetRouteData(),
        new ActionDescriptor());

    var executor = httpContext.RequestServices.GetRequiredService<IActionResultExecutor<ObjectResult>>();
    return executor.ExecuteAsync(actionContext, result);
}
