using System.Security.Cryptography;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PokeMarketApi.Data;
using PokeMarketApi.Models;

namespace PokeMarketApi.Controllers;

[Authorize]
public class ApiRequestController(MarketContext db, UserManager<IdentityUser> users) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await users.GetUserAsync(User);
        return View(new ApiRequestViewModel { ContactEmail = user?.Email ?? "" });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ApiRequestViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Challenge();

        // The raw key is shown only in this response. Only its SHA-256 hash is persisted.
        var rawKey = "pm_live_" + Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');
        var keyHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));

        db.ApiClientRegistrations.Add(new ApiClientRegistration
        {
            UserId = userId,
            ApplicationName = model.ApplicationName.Trim(),
            ContactEmail = model.ContactEmail.Trim(),
            Website = string.IsNullOrWhiteSpace(model.Website) ? null : model.Website.Trim(),
            Purpose = model.Purpose.Trim(),
            KeyHash = keyHash,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        model.ApiUrl = $"{Request.Scheme}://{Request.Host}/api/v1/cards";
        model.ApiKey = rawKey;
        model.Submitted = true;
        return View(model);
    }
}
