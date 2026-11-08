using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PokemonMvc.Data;

namespace PokemonMvc.Controllers;

public class HomeController(PokemonContext db) : Controller
{
    public async Task<IActionResult> Index(string? searchString, string? sortOrder, string? setFilter, int page = 1)
    {
        const int pageSize = 12;
        var query = db.Cards.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(searchString))
        {
            var term = searchString.Trim();
            query = query.Where(x => x.Name.Contains(term) || x.SetName.Contains(term) ||
                x.CardNumber.Contains(term) || x.Rarity.Contains(term) || x.ProductType.Contains(term));
        }
        if (!string.IsNullOrWhiteSpace(setFilter)) query = query.Where(x => x.SetName == setFilter);
        query = sortOrder switch
        {
            "name_desc" => query.OrderByDescending(x => x.Name),
            "price_asc" => query.OrderBy(x => x.MarketPriceQar),
            "price_desc" => query.OrderByDescending(x => x.MarketPriceQar),
            "set_asc" => query.OrderBy(x => x.SetName).ThenBy(x => x.Name),
            "set_desc" => query.OrderByDescending(x => x.SetName).ThenBy(x => x.Name),
            "name_asc" => query.OrderBy(x => x.Name),
            _ => query.OrderBy(x => x.ProductType).ThenBy(x => x.Name)
        };
        var totalCount = await query.CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize));
        page = Math.Clamp(page, 1, totalPages);
        ViewBag.SearchString = searchString ?? "";
        ViewBag.SortOrder = sortOrder ?? "";
        ViewBag.SetFilter = setFilter ?? "";
        ViewBag.Sets = await db.Cards.AsNoTracking().Select(x => x.SetName).Distinct().OrderBy(x => x).ToListAsync();
        ViewBag.TotalCount = totalCount;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        return View(await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync());
    }
}