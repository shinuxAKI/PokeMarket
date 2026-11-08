using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PokemonMvc.Data;

namespace PokemonMvc.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController(PokemonContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        ViewBag.TotalProducts = await db.Cards.CountAsync();
        ViewBag.TotalCards = await db.Cards.CountAsync(x => x.ProductType == "Card");
        ViewBag.BoosterPacks = await db.Cards.CountAsync(x => x.ProductType == "Booster Pack");
        ViewBag.TrainerBoxes = await db.Cards.CountAsync(x => x.ProductType == "Trainer Box");
        ViewBag.CatalogueValue = await db.Cards.Select(x => (decimal?)x.MarketPriceQar).SumAsync() ?? 0m;
        ViewBag.Sets = await db.Cards.Select(x => x.SetName).Distinct().CountAsync();
        ViewBag.RecentProducts = await db.Cards.OrderByDescending(x => x.Id).Take(6).ToListAsync();
        ViewBag.TopSets = await db.Cards.GroupBy(x => x.SetName)
            .Select(g => new SetSummary { Name = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count).Take(6).ToListAsync();
        return View();
    }

    public sealed class SetSummary
    {
        public string Name { get; set; } = "";
        public int Count { get; set; }
    }
}