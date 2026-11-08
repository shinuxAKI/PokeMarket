using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PokemonMvc.Data;
using PokemonMvc.Models;

namespace PokemonMvc.Controllers;

public class CardsController(PokemonContext db) : Controller
{
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Index(string? search)
    {
        var query = db.Cards.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => x.Name.Contains(search) || x.CardNumber.Contains(search));
        return View(await query.OrderBy(x => x.Name).ToListAsync());
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var card = await db.Cards.FindAsync(id);
        return card == null ? NotFound() : View(card);
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Create() => View(new Card());

    [Authorize(Roles = "Admin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Card card)
    {
        if (!ModelState.IsValid) return View(card);
        db.Cards.Add(card);
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var card = await db.Cards.FindAsync(id);
        return card == null ? NotFound() : View(card);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Card card)
    {
        if (id != card.Id) return BadRequest();
        if (!ModelState.IsValid) return View(card);

        var existing = await db.Cards.FindAsync(id);
        if (existing == null) return NotFound();

        existing.Name = card.Name;
        existing.CardNumber = card.CardNumber;
        existing.SetName = card.SetName;
        existing.Language = card.Language;
        existing.ProductType = card.ProductType;
        existing.Rarity = card.Rarity;
        existing.MarketPriceQar = card.MarketPriceQar;
        existing.ImageUrl = card.ImageUrl;

        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var card = await db.Cards.FindAsync(id);
        return card == null ? NotFound() : View(card);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var card = await db.Cards.FindAsync(id);
        if (card != null) { db.Remove(card); await db.SaveChangesAsync(); }
        return RedirectToAction(nameof(Index));
    }
}
