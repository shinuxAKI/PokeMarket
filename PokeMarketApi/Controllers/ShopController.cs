using Microsoft.AspNetCore.Mvc;
using PokeMarketApi.Services;

namespace PokeMarketApi.Controllers;

public class ShopController(CatalogClient catalog) : Controller
{
    public async Task<IActionResult> Index()
    {
        try
        {
            return View(await catalog.GetCards());
        }
        catch
        {
            ViewBag.Offline = true;
            return View(new List<CatalogCard>());
        }
    }
}