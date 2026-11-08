using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PokeMarketApi.Data;
using PokeMarketApi.Models;

namespace PokeMarketApi.Controllers;

[Authorize]
public class FavouritesController(MarketContext db, UserManager<IdentityUser> users) : Controller
{
    public async Task<IActionResult> Index()
    {
        var id = users.GetUserId(User)!;
        return View(await db.FavouriteItems.Where(x => x.UserId == id).OrderBy(x => x.ProductName).ToListAsync());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int catalogCardId, string productName, string? imageUrl, string? setName, decimal priceQar)
    {
        var userId = users.GetUserId(User)!;
        var existing = await db.FavouriteItems.FirstOrDefaultAsync(x => x.UserId == userId && x.CatalogCardId == catalogCardId);
        if (existing != null) db.FavouriteItems.Remove(existing);
        else db.FavouriteItems.Add(new FavouriteItem
        {
            UserId = userId, CatalogCardId = catalogCardId, ProductName = productName,
            ImageUrl = imageUrl, SetName = setName, PriceQar = priceQar
        });
        await db.SaveChangesAsync();
        return Redirect(Request.Headers.Referer.ToString().Contains("/Search", StringComparison.OrdinalIgnoreCase)
            ? "/Search" : "/Favourites");
    }
}