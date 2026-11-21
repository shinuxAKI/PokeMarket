namespace PokeMarketApi.Models;

public class ApiClientRegistration
{
    public int Id { get; set; }
    public string UserId { get; set; } = "";
    public string ApplicationName { get; set; } = "";
    public string ContactEmail { get; set; } = "";
    public string? Website { get; set; }
    public string Purpose { get; set; } = "";
    public string KeyHash { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
