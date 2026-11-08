namespace PokeMarketApi.Models;

public class PriceSnapshot
{
    public int Id { get; set; }
    public int CatalogCardId { get; set; }
    public string ProductName { get; set; } = "";
    public decimal PriceQar { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    public bool IsDemoData { get; set; } = true;
}