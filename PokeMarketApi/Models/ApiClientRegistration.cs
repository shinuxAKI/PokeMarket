namespace PokeMarketApi.Models;

public class ApiClientRegistration
{
    public int Id { get; set; }
    public string UserId { get; set; } = "";
    public string ApplicationName { get; set; } = "";
    public string ContactEmail { get; set; } = "";
    public string? Website { get; set; }
    public string Purpose { get; set; } = "";
    // A unique placeholder is stored while pending so the existing SQLite unique index remains valid.
    public string KeyHash { get; set; } = "";
    public string? ProtectedKey { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewedBy { get; set; }
    public string? AdminNotes { get; set; }
}
