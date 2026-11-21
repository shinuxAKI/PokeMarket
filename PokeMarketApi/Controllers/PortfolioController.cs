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
    private static readonly string[] ValidConditions =
        ["Near Mint", "Lightly Played", "Moderately Played", "Heavily Played", "Damaged"];

    private static bool IsValidGrade(string? grade) =>
        grade == "Ungraded" || (grade != null && grade.StartsWith("PSA ") &&
        int.TryParse(grade[4..], out var value) && value is >= 1 and <= 10);

    private static bool IsValidCondition(string? condition) =>
        condition != null && ValidConditions.Contains(condition);

    public async Task<IActionResult> Index() =>
        View(await db.PortfolioItems.Where(x => x.UserId == users.GetUserId(User))
            .OrderBy(x => x.ProductName).ThenBy(x => x.Grade).ToListAsync());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(string productName, string? imageUrl, string? setName, string grade,
        decimal currentValueQar, int quantity = 1, string condition = "Near Mint", string? returnUrl = null)
    {
        productName = (productName ?? "").Trim();
        setName = string.IsNullOrWhiteSpace(setName) ? null : setName.Trim();

        if (productName.Length == 0 || productName.Length > 120 ||
            (setName?.Length > 120) || quantity < 1 || quantity > 999 ||
            currentValueQar < 0 || currentValueQar > 1000000 ||
            !IsValidGrade(grade) || !IsValidCondition(condition))
            return BadRequest("Check the product name, quantity, value, grade and condition.");

        var userId = users.GetUserId(User)!;
        var existing = await db.PortfolioItems.FirstOrDefaultAsync(x =>
            x.UserId == userId &&
            x.ProductName == productName &&
            x.SetName == setName &&
            x.Grade == grade &&
            x.Condition == condition);

        if (existing != null)
        {
            if (existing.Quantity + quantity > 999)
                return BadRequest("This collection entry cannot exceed 999 copies.");
            existing.Quantity += quantity;
            existing.CurrentValueQar = currentValueQar;
            if (!string.IsNullOrWhiteSpace(imageUrl)) existing.ImageUrl = imageUrl;
        }
        else
        {
            db.PortfolioItems.Add(new PortfolioItem
            {
                UserId = userId,
                ProductName = productName,
                ImageUrl = imageUrl,
                SetName = setName,
                Grade = grade,
                Condition = condition,
                Quantity = quantity,
                CurrentValueQar = currentValueQar
            });
        }

        await db.SaveChangesAsync();
        TempData["PortfolioMessage"] = existing == null
            ? "Added to your collection."
            : $"Added {quantity} more. Your collection quantity is now {existing.Quantity}.";

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int id, int quantity, string condition, string grade)
    {
        var item = await db.PortfolioItems.FirstOrDefaultAsync(x =>
            x.Id == id && x.UserId == users.GetUserId(User));
        if (item == null) return NotFound();
        if (quantity < 1 || quantity > 999 || !IsValidGrade(grade) || !IsValidCondition(condition))
            return BadRequest("Check the quantity, grade and condition.");

        item.Quantity = quantity;
        item.Condition = condition;
        item.Grade = grade;
        await db.SaveChangesAsync();
        TempData["PortfolioMessage"] = "Collection item updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int id)
    {
        var item = await db.PortfolioItems.FirstOrDefaultAsync(x =>
            x.Id == id && x.UserId == users.GetUserId(User));
        if (item != null)
        {
            db.PortfolioItems.Remove(item);
            await db.SaveChangesAsync();
            TempData["PortfolioMessage"] = "Item removed from your collection.";
        }
        return RedirectToAction(nameof(Index));
    }
}