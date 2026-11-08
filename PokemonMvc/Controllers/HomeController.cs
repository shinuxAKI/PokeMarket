using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PokemonMvc.Data;

namespace PokemonMvc.Controllers;

public class HomeController(PokemonContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var cards = await db.Cards
            .OrderBy(x => x.ProductType)
            .ThenBy(x => x.Name)
            .ToListAsync();

        return View(cards);
    }
}
