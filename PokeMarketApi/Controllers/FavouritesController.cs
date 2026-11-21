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
        var userId = users.GetUserId(User)!;
        return View(await db.FavouriteItems.Where(x => x.UserId == userId)
            .OrderBy(x => x.ProductName).ToListAsync());
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
        if (catalogCardId <= 0)
            return BadRequest("Invalid catalogue product.");

        var userId = users.GetUserId(User)!;
        var existing = await db.FavouriteItems.FirstOrDefaultAsync(
            x => x.UserId == userId && x.CatalogCardId == catalogCardId);

        if (existing != null)
        {
            db.FavouriteItems.Remove(existing);
            TempData["FavouriteMessage"] = "Removed from your favourites.";
        }
        else
        {
            productName = (productName ?? "").Trim();
            if (productName.Length == 0 || productName.Length > 120 ||
                (setName?.Length > 120) || priceQar < 0 || priceQar > 1000000)
                return BadRequest("The product information is invalid. Please return to the catalogue and try again.");

            db.FavouriteItems.Add(new FavouriteItem
            {
                UserId = userId,
                CatalogCardId = catalogCardId,
                ProductName = productName,
                ImageUrl = imageUrl,
                SetName = setName,
                PriceQar = priceQar
            });
            TempData["FavouriteMessage"] = "Added to your favourites.";
        }

        await db.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);

        return RedirectToAction(nameof(Index));
    }
}