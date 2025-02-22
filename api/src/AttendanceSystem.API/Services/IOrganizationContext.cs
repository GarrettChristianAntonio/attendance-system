namespace AttendanceSystem.API.Services;

public interface IOrganizationContext
{
    Guid OrganizationId { get; set; }
    string OrganizationName { get; set; }
}

public class OrganizationContext : IOrganizationContext
{
    public Guid OrganizationId { get; set; }
    public string OrganizationName { get; set; } = string.Empty;
}
