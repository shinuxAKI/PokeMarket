using Microsoft.AspNetCore.Mvc;
using PokeMarketApi.Services;

namespace PokeMarketApi.Controllers;

public class SearchController(CatalogClient catalog) : Controller
{
    public async Task<IActionResult> Index(string? q, string? setName, string? language, string? productType, string? sort)
    {
        try
        {
            var cards = await catalog.GetCards();
            if (!string.IsNullOrWhiteSpace(q))
                cards = cards.Where(x => x.Name.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    x.CardNumber.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();
            if (!string.IsNullOrWhiteSpace(setName)) cards = cards.Where(x => x.SetName == setName).ToList();
            if (!string.IsNullOrWhiteSpace(language)) cards = cards.Where(x => x.Language == language).ToList();
            if (!string.IsNullOrWhiteSpace(productType)) cards = cards.Where(x => x.ProductType == productType).ToList();
            cards = sort switch
            {
                "price-low" => cards.OrderBy(x => x.MarketPriceQar).ToList(),
                "price-high" => cards.OrderByDescending(x => x.MarketPriceQar).ToList(),
                "name" => cards.OrderBy(x => x.Name).ToList(),
                _ => cards.OrderBy(x => x.Name).ToList()
            };
            ViewBag.Sets = await catalog.GetSets();
            ViewBag.Offline = false;
            return View(cards);
        }
        catch
        {
            ViewBag.Sets = new List<string>();
            ViewBag.Offline = true;
            return View(new List<CatalogCard>());
        }
    }
}