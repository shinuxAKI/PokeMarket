using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PokeMarketApi.Data;

namespace PokeMarketApi.Controllers;

[Authorize]
public class ProfileController(MarketContext db, UserManager<IdentityUser> users) : Controller
{
    public async Task<IActionResult> Index()
    {
        var id = users.GetUserId(User)!;
        ViewBag.Email = User.Identity?.Name;
        ViewBag.Items = await db.PortfolioItems.CountAsync(x => x.UserId == id);
        ViewBag.TotalCards = await db.PortfolioItems.Where(x => x.UserId == id).SumAsync(x => (int?)x.Quantity) ?? 0;
        ViewBag.Value = await db.PortfolioItems.Where(x => x.UserId == id).SumAsync(x => (decimal?)(x.CurrentValueQar * x.Quantity)) ?? 0m;
        ViewBag.Listings = await db.SellerListings.CountAsync(x => x.SellerId == id);
        ViewBag.Favourites = await db.FavouriteItems.CountAsync(x => x.UserId == id);
        return View();
    }
}