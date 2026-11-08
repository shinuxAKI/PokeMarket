using System.ComponentModel.DataAnnotations;

namespace PokeMarketApi.Models;

public class ProductReview
{
    public int Id { get; set; }
    public int CatalogCardId { get; set; }
    public string UserId { get; set; } = "";
    public string UserEmail { get; set; } = "";
    [Range(1, 5)] public int Rating { get; set; }
    [MaxLength(800)] public string Comment { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}