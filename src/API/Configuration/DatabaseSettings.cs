namespace API.Configuration;

public sealed record DatabaseSettings(string? ConnectionString)
{
    public bool IsConfigured => !string.IsNullOrWhiteSpace(ConnectionString);
}
