namespace AttendanceSystem.API.DTOs;

public class CreateEmbedTokenRequest
{
    public string Name { get; set; } = string.Empty;
    public string[] Scopes { get; set; } = [];
    public int ExpiresInDays { get; set; } = 30;
}

public class EmbedTokenResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

public class EmbedTokenListItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Scopes { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
