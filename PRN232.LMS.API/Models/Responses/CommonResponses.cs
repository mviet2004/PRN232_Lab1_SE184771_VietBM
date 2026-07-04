namespace PRN232.LMS.API.Models.Responses;

public class EmptyResponse
{
}

public class HealthDatabaseResponse
{
    public bool Connected { get; set; }
    public string Database { get; set; } = string.Empty;
    public string DataSource { get; set; } = string.Empty;
}
