using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PokeMarketApi.Data;
using PokeMarketApi.Models;
using PokeMarketApi.Services;

namespace PokeMarketApi.Controllers;

public class ProductsController(CatalogClient catalog, MarketContext db, UserManager<IdentityUser> users) : Controller
{
    public async Task<IActionResult> Details(int id)
    {
        CatalogCard? card;
        try { card = await catalog.GetCard(id); }
        catch { return StatusCode(503, "Catalogue offline. Start PokemonMvc and check CatalogApi:BaseUrl."); }
        if (card == null) return NotFound();

        var history = await db.PriceSnapshots.Where(x => x.CatalogCardId == id)
            .OrderBy(x => x.RecordedAt).ToListAsync();
        if (history.Count == 0)
        {
            // Clearly labelled illustrative history for the demo; these are not verified market records.
            var now = DateTime.UtcNow;
            var demo = new[]
            {
                new PriceSnapshot { CatalogCardId=id, ProductName=card.Name, PriceQar=Math.Round(card.MarketPriceQar * .92m, 2), RecordedAt=now.AddDays(-30), IsDemoData=true },
                new PriceSnapshot { CatalogCardId=id, ProductName=card.Name, PriceQar=Math.Round(card.MarketPriceQar * .96m, 2), RecordedAt=now.AddDays(-21), IsDemoData=true },
                new PriceSnapshot { CatalogCardId=id, ProductName=card.Name, PriceQar=Math.Round(card.MarketPriceQar * .94m, 2), RecordedAt=now.AddDays(-14), IsDemoData=true },
                new PriceSnapshot { CatalogCardId=id, ProductName=card.Name, PriceQar=Math.Round(card.MarketPriceQar * 1.02m, 2), RecordedAt=now.AddDays(-7), IsDemoData=true },
                new PriceSnapshot { CatalogCardId=id, ProductName=card.Name, PriceQar=card.MarketPriceQar, RecordedAt=now, IsDemoData=true }
            };
            db.PriceSnapshots.AddRange(demo);
            await db.SaveChangesAsync();
            history = demo.ToList();
        }

        ViewBag.PriceHistory = history;
        ViewBag.Reviews = await db.ProductReviews.Where(x => x.CatalogCardId == id)
            .OrderByDescending(x => x.CreatedAt).ToListAsync();
        ViewBag.AverageRating = await db.ProductReviews.Where(x => x.CatalogCardId == id)
            .Select(x => (double?)x.Rating).AverageAsync();
        ViewBag.IsFavourite = User.Identity?.IsAuthenticated == true &&
            await db.FavouriteItems.AnyAsync(x => x.UserId == users.GetUserId(User) && x.CatalogCardId == id);
        return View(card);
    }
}