using System.Security.Cryptography;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PokeMarketApi.Data;
using PokeMarketApi.Models;

namespace PokeMarketApi.Controllers;

[Authorize]
public class ApiRequestController(
    MarketContext db,
    UserManager<IdentityUser> users,
    IDataProtectionProvider protectionProvider) : Controller
{
    private readonly IDataProtector keyProtector = protectionProvider.CreateProtector("PokeMarket.ApiKeys.v1");

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await users.GetUserAsync(User);
        var page = await BuildPageAsync(users.GetUserId(User)!);
        page.Form.ContactEmail = user?.Email ?? "";
        return View(page);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ApiRequestPageViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var invalidPage = await BuildPageAsync(users.GetUserId(User)!);
            invalidPage.Form = model.Form;
            return View(invalidPage);
        }

        var userId = users.GetUserId(User)!;
        db.ApiClientRegistrations.Add(new ApiClientRegistration
        {
            UserId = userId,
            ApplicationName = model.Form.ApplicationName.Trim(),
            ContactEmail = model.Form.ContactEmail.Trim(),
            Website = string.IsNullOrWhiteSpace(model.Form.Website) ? null : model.Form.Website.Trim(),
            Purpose = model.Form.Purpose.Trim(),
            KeyHash = "pending_" + Guid.NewGuid().ToString("N"),
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        TempData["ApiRequestMessage"] = "Your API access request was submitted. An administrator must approve it before a key is issued.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Rotate(int id)
    {
        var request = await db.ApiClientRegistrations.FirstOrDefaultAsync(x =>
            x.Id == id && x.UserId == users.GetUserId(User));
        if (request == null) return NotFound();
        if (request.Status != "Approved")
            return BadRequest("Only approved API requests can have their keys rotated.");

        var rawKey = GenerateApiKey();
        request.KeyHash = HashKey(rawKey);
        request.ProtectedKey = keyProtector.Protect(rawKey);
        await db.SaveChangesAsync();

        TempData["ApiRequestMessage"] = "Your API key was rotated. Your new key is now visible in your API Access page.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Revoke(int id)
    {
        var request = await db.ApiClientRegistrations.FirstOrDefaultAsync(x =>
            x.Id == id && x.UserId == users.GetUserId(User));
        if (request == null) return NotFound();
        request.Status = "Revoked";
        request.KeyHash = "revoked_" + Guid.NewGuid().ToString("N");
        request.ProtectedKey = null;
        request.ReviewedAt = DateTime.UtcNow;
        request.ReviewedBy = users.GetUserId(User);
        await db.SaveChangesAsync();

        TempData["ApiRequestMessage"] = "Your API key has been revoked.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<ApiRequestPageViewModel> BuildPageAsync(string userId)
    {
        var requests = await db.ApiClientRegistrations.AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return new ApiRequestPageViewModel
        {
            ApiUrl = $"{Request.Scheme}://{Request.Host}/api/v1/cards",
            Requests = requests.Select(x => new ApiRequestStatusViewModel
            {
                Id = x.Id,
                ApplicationName = x.ApplicationName,
                Status = x.Status,
                CreatedAt = x.CreatedAt,
                AdminNotes = x.AdminNotes,
                ApiKey = x.Status == "Approved" && !string.IsNullOrWhiteSpace(x.ProtectedKey)
                    ? TryUnprotect(x.ProtectedKey)
                    : null,
                NeedsKeyRotation = x.Status == "Approved" && string.IsNullOrWhiteSpace(x.ProtectedKey)
            }).ToList()
        };
    }

    private string? TryUnprotect(string protectedKey)
    {
        try { return keyProtector.Unprotect(protectedKey); }
        catch (CryptographicException) { return null; }
    }

    internal static string GenerateApiKey() =>
        "pm_live_" + Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');

    internal static string HashKey(string apiKey) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(apiKey)));
}
