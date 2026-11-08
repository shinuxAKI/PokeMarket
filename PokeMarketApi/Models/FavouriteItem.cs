namespace PokeMarketApi.Models;
public class FavouriteItem
{
    public int Id { get; set; }
    public string UserId { get; set; } = "";
    public int CatalogCardId { get; set; }
    public string ProductName { get; set; } = "";
    public string? ImageUrl { get; set; }
    public string? SetName { get; set; }
    public decimal PriceQar { get; set; }
}