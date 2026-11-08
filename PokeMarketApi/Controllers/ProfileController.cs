using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PokeMarketApi.Data;
using Microsoft.EntityFrameworkCore;

namespace PokeMarketApi.Controllers;

[Authorize]
public class ProfileController(MarketContext db, UserManager<IdentityUser> users) : Controller
{
    public async Task<IActionResult> Index()
    {
        var id = users.GetUserId(User)!;
        ViewBag.Email = User.Identity?.Name;
        ViewBag.Items = await db.PortfolioItems.CountAsync(x => x.UserId == id);
        ViewBag.Value = await db.PortfolioItems.Where(x => x.UserId == id).SumAsync(x => (decimal?)x.CurrentValueQar) ?? 0m;
        ViewBag.Listings = await db.SellerListings.CountAsync(x => x.SellerId == id);
        return View();
    }
}