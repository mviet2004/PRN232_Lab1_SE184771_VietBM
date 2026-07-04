using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace PRN232.LMS.API.Common;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }

    [XmlIgnore]
    public object? Errors { get; set; }

    [JsonIgnore]
    [XmlArray("Errors")]
    [XmlArrayItem("Error")]
    public List<ApiErrorDetail>? XmlErrors
    {
        get
        {
            var errors = ApiErrorDetail.From(Errors);
            return errors.Count == 0 ? null : errors;
        }
        set
        {
        }
    }

    public static ApiResponse<T> Ok(T data, string message = "Request processed successfully") =>
        new() { Success = true, Message = message, Data = data };

    public static ApiResponse<T> Fail(string message, object? errors = null) =>
        new() { Success = false, Message = message, Errors = errors };

    public static ApiResponse<T> ValidationFail(ModelStateDictionary modelState, string message = "Invalid request") =>
        Fail(message, modelState
            .Where(x => x.Value?.Errors.Count > 0)
            .ToDictionary(
                x => x.Key,
                x => x.Value!.Errors.Select(e => e.ErrorMessage).ToArray()));
}

public class ApiErrorDetail
{
    public string Field { get; set; } = string.Empty;

    [XmlArrayItem("Message")]
    public List<string> Messages { get; set; } = new();

    public static List<ApiErrorDetail> From(object? errors)
    {
        if (errors is null)
        {
            return new List<ApiErrorDetail>();
        }

        if (errors is string message)
        {
            return new List<ApiErrorDetail>
            {
                new() { Messages = new List<string> { message } }
            };
        }

        if (errors is IDictionary<string, string> stringErrors)
        {
            return stringErrors
                .Select(error => new ApiErrorDetail
                {
                    Field = error.Key,
                    Messages = new List<string> { error.Value }
                })
                .ToList();
        }

        if (errors is IDictionary<string, string[]> validationErrors)
        {
            return validationErrors
                .Select(error => new ApiErrorDetail
                {
                    Field = error.Key,
                    Messages = error.Value.ToList()
                })
                .ToList();
        }

        return new List<ApiErrorDetail>
        {
            new() { Messages = new List<string> { errors.ToString() ?? string.Empty } }
        };
    }
}
