using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PokeMarketApi.Data;
using PokeMarketApi.Models;

namespace PokeMarketApi.Controllers;

[Authorize]
public class PortfolioController(MarketContext db, UserManager<IdentityUser> users) : Controller
{
    public async Task<IActionResult> Index() =>
        View(await db.PortfolioItems.Where(x => x.UserId == users.GetUserId(User)).OrderBy(x => x.ProductName).ToListAsync());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(string productName, string? imageUrl, string? setName, string grade,
        decimal currentValueQar, int quantity = 1, string condition = "Near Mint")
    {
        if (string.IsNullOrWhiteSpace(productName) || quantity < 1 || quantity > 999 || currentValueQar < 0)
            return BadRequest("Enter a product name, a valid quantity and a non-negative value.");
        db.PortfolioItems.Add(new PortfolioItem
        {
            UserId = users.GetUserId(User)!, ProductName = productName.Trim(), ImageUrl = imageUrl,
            SetName = setName, Grade = grade, Condition = condition, Quantity = quantity,
            CurrentValueQar = currentValueQar
        });
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int id, int quantity, string condition, string grade)
    {
        var item = await db.PortfolioItems.FirstOrDefaultAsync(x => x.Id == id && x.UserId == users.GetUserId(User));
        if (item == null) return NotFound();
        if (quantity < 1 || quantity > 999) return BadRequest("Quantity must be between 1 and 999.");
        item.Quantity = quantity;
        item.Condition = condition;
        item.Grade = grade;
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int id)
    {
        var item = await db.PortfolioItems.FirstOrDefaultAsync(x => x.Id == id && x.UserId == users.GetUserId(User));
        if (item != null) { db.Remove(item); await db.SaveChangesAsync(); }
        return RedirectToAction(nameof(Index));
    }
}