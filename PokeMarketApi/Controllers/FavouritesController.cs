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
    public async Task<IActionResult> Toggle(
        int catalogCardId,
        string productName,
        string? imageUrl,
        string? setName,
        decimal priceQar,
        string? returnUrl)
    {
        var userId = users.GetUserId(User)!;
        var existing = await db.FavouriteItems.FirstOrDefaultAsync(
            x => x.UserId == userId && x.CatalogCardId == catalogCardId);

        if (existing != null)
        {
            db.FavouriteItems.Remove(existing);
        }
        else
        {
            db.FavouriteItems.Add(new FavouriteItem
            {
                UserId = userId,
                CatalogCardId = catalogCardId,
                ProductName = productName,
                ImageUrl = imageUrl,
                SetName = setName,
                PriceQar = priceQar
            });
        }

        await db.SaveChangesAsync();

        // Only redirect to local paths supplied by the app; never trust an external return URL.
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);

        return RedirectToAction(nameof(Index));
    }
}