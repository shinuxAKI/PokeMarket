using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PokeMarketApi.Data;
using PokeMarketApi.Models;

namespace PokeMarketApi.Controllers;

[Authorize]
public class SellController(MarketContext db, UserManager<IdentityUser> users, IWebHostEnvironment environment) : Controller
{
    public async Task<IActionResult> Index()
    {
        ViewBag.CurrentUserId = users.GetUserId(User);
        return View(await db.SellerListings.OrderByDescending(x => x.CreatedAt).ToListAsync());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string productName, string productType, string grade,
        string condition, decimal priceQar, IFormFile? photo)
    {
        productName = (productName ?? "").Trim();
        if (productName.Length == 0 || productName.Length > 120 || priceQar <= 0 || priceQar > 1000000)
            return BadRequest("Enter a product name and a valid positive QAR price.");

        string? imageUrl = null;
        if (photo is { Length: > 0 })
        {
            if (photo.Length > 5 * 1024 * 1024)
                return BadRequest("Image must be 5 MB or smaller.");

            var extension = Path.GetExtension(photo.FileName).ToLowerInvariant();
            var allowedTypes = new Dictionary<string, string>
            {
                [".jpg"] = "image/jpeg",
                [".jpeg"] = "image/jpeg",
                [".png"] = "image/png",
                [".webp"] = "image/webp"
            };
            if (!allowedTypes.TryGetValue(extension, out var expectedType) ||
                !string.Equals(photo.ContentType, expectedType, StringComparison.OrdinalIgnoreCase))
                return BadRequest("Upload a JPG, PNG or WEBP image file.");

            var webRoot = environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot");
            var folder = Path.Combine(webRoot, "uploads");
            Directory.CreateDirectory(folder);
            var filename = Guid.NewGuid().ToString("N") + extension;
            var fullPath = Path.Combine(folder, filename);
            await using (var stream = System.IO.File.Create(fullPath))
                await photo.CopyToAsync(stream);
            imageUrl = "/uploads/" + filename;
        }

        db.SellerListings.Add(new SellerListing
        {
            SellerId = users.GetUserId(User)!,
            ProductName = productName,
            ProductType = productType,
            ImageUrl = imageUrl,
            Grade = grade,
            Condition = condition,
            PriceQar = priceQar
        });
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int id, string productName, string grade, string condition, decimal priceQar, string status)
    {
        var listing = await db.SellerListings.FirstOrDefaultAsync(x => x.Id == id && x.SellerId == users.GetUserId(User));
        if (listing == null) return NotFound();
        productName = (productName ?? "").Trim();
        if (productName.Length == 0 || productName.Length > 120 || priceQar <= 0 || priceQar > 1000000)
            return BadRequest("Enter a valid product name and price.");
        listing.ProductName = productName;
        listing.Grade = grade;
        listing.Condition = condition;
        listing.PriceQar = priceQar;
        listing.Status = status == "Sold" ? "Sold" : "Available";
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var listing = await db.SellerListings.FirstOrDefaultAsync(x => x.Id == id && x.SellerId == users.GetUserId(User));
        if (listing != null)
        {
            if (!string.IsNullOrWhiteSpace(listing.ImageUrl) && listing.ImageUrl.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
            {
                var webRoot = environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot");
                var filename = Path.GetFileName(listing.ImageUrl);
                var imagePath = Path.Combine(webRoot, "uploads", filename);
                if (System.IO.File.Exists(imagePath)) System.IO.File.Delete(imagePath);
            }
            db.SellerListings.Remove(listing);
            await db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}