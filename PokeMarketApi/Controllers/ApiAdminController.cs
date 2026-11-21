using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PokeMarketApi.Data;
using PokeMarketApi.Models;

namespace PokeMarketApi.Controllers;

[Authorize(Roles = "Admin")]
public class ApiAdminController(
    MarketContext db,
    UserManager<IdentityUser> users,
    IDataProtectionProvider protectionProvider) : Controller
{
    private readonly IDataProtector keyProtector = protectionProvider.CreateProtector("PokeMarket.ApiKeys.v1");

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var requests = await db.ApiClientRegistrations.AsNoTracking()
            .OrderBy(x => x.Status == "Pending" ? 0 : 1)
            .ThenByDescending(x => x.CreatedAt)
            .ToListAsync();
        return View(requests);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        var request = await db.ApiClientRegistrations.FindAsync(id);
        if (request == null) return NotFound();
        if (request.Status != "Pending" && request.Status != "Rejected")
        {
            TempData["ApiAdminMessage"] = "Only pending or rejected requests can be approved.";
            return RedirectToAction(nameof(Index));
        }

        var key = ApiRequestController.GenerateApiKey();
        request.KeyHash = ApiRequestController.HashKey(key);
        request.ProtectedKey = keyProtector.Protect(key);
        request.Status = "Approved";
        request.AdminNotes = null;
        request.ReviewedAt = DateTime.UtcNow;
        request.ReviewedBy = users.GetUserId(User);
        await db.SaveChangesAsync();

        TempData["ApiAdminMessage"] = $"Approved {request.ApplicationName}. The user can now view their API credentials on their API Access page.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id, string? adminNotes)
    {
        var request = await db.ApiClientRegistrations.FindAsync(id);
        if (request == null) return NotFound();
        if (request.Status != "Pending")
        {
            TempData["ApiAdminMessage"] = "Only pending requests can be rejected.";
            return RedirectToAction(nameof(Index));
        }

        request.Status = "Rejected";
        request.AdminNotes = string.IsNullOrWhiteSpace(adminNotes) ? null : adminNotes.Trim()[..Math.Min(adminNotes.Trim().Length, 500)];
        request.KeyHash = "rejected_" + Guid.NewGuid().ToString("N");
        request.ProtectedKey = null;
        request.ReviewedAt = DateTime.UtcNow;
        request.ReviewedBy = users.GetUserId(User);
        await db.SaveChangesAsync();

        TempData["ApiAdminMessage"] = $"Rejected {request.ApplicationName}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Revoke(int id)
    {
        var request = await db.ApiClientRegistrations.FindAsync(id);
        if (request == null) return NotFound();
        if (request.Status != "Approved")
        {
            TempData["ApiAdminMessage"] = "Only approved API keys can be revoked.";
            return RedirectToAction(nameof(Index));
        }

        request.Status = "Revoked";
        request.KeyHash = "revoked_" + Guid.NewGuid().ToString("N");
        request.ProtectedKey = null;
        request.ReviewedAt = DateTime.UtcNow;
        request.ReviewedBy = users.GetUserId(User);
        await db.SaveChangesAsync();

        TempData["ApiAdminMessage"] = $"Revoked the API key for {request.ApplicationName}.";
        return RedirectToAction(nameof(Index));
    }
}
