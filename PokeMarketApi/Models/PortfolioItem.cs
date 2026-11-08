namespace PokeMarketApi.Models;

public class PortfolioItem
{
    public int Id { get; set; }
    public string UserId { get; set; } = "";
    public string ProductName { get; set; } = "";
    public string? ImageUrl { get; set; }
    public string? SetName { get; set; }
    public string Grade { get; set; } = "Ungraded";
    public string Condition { get; set; } = "Near Mint";
    public int Quantity { get; set; } = 1;
    public decimal CurrentValueQar { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}