using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PokemonMvc.Data;
using PokemonMvc.Models;

namespace PokemonMvc.Controllers;

public class CardsController(PokemonContext db, IWebHostEnvironment env) : Controller
{
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Index(string? search)
    {
        var query = db.Cards.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => x.Name.Contains(search) || x.CardNumber.Contains(search));
        return View(await query.OrderBy(x => x.Name).ToListAsync());
    }

    // Product pages are public; only management actions require the Admin role.
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
    public async Task<IActionResult> Create(Card card, IFormFile? photo)
    {
        if (!ModelState.IsValid) return View(card);
        await Save(card, photo);
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
    public async Task<IActionResult> Edit(int id, Card card, IFormFile? photo)
    {
        if (id != card.Id) return BadRequest();
        if (!ModelState.IsValid) return View(card);
        await Save(card, photo);
        if (!ModelState.IsValid) return View(card);
        db.Update(card);
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

    private async Task Save(Card card, IFormFile? photo)
    {
        if (photo == null || photo.Length == 0) return;
        if (photo.Length > 5 * 1024 * 1024) { ModelState.AddModelError("photo", "Image must be 5 MB or smaller."); return; }
        var extension = Path.GetExtension(photo.FileName).ToLowerInvariant();
        if (!new[] { ".jpg", ".jpeg", ".png", ".webp" }.Contains(extension))
        { ModelState.AddModelError("photo", "Use a JPG, PNG or WEBP image."); return; }
        var directory = Path.Combine(env.WebRootPath, "uploads");
        Directory.CreateDirectory(directory);
        var filename = Guid.NewGuid() + extension;
        await using var stream = System.IO.File.Create(Path.Combine(directory, filename));
        await photo.CopyToAsync(stream);
        card.ImageUrl = "/uploads/" + filename;
    }
}
