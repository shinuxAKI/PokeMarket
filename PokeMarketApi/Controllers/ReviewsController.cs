using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PokeMarketApi.Data;
using PokeMarketApi.Models;

namespace PokeMarketApi.Controllers;

[Authorize]
public class ReviewsController(MarketContext db, UserManager<IdentityUser> users) : Controller
{
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int catalogCardId, int rating, string comment)
    {
        comment = (comment ?? "").Trim();
        if (catalogCardId <= 0 || rating is < 1 or > 5 ||
            comment.Length == 0 || comment.Length > 800)
            return BadRequest("Write a review and choose a rating from 1 to 5 (maximum 800 characters).");

        var userId = users.GetUserId(User)!;
        var review = await db.ProductReviews.FirstOrDefaultAsync(
            x => x.CatalogCardId == catalogCardId && x.UserId == userId);

        if (review == null)
        {
            review = new ProductReview
            {
                CatalogCardId = catalogCardId,
                UserId = userId,
                UserEmail = User.Identity?.Name ?? "Collector"
            };
            db.ProductReviews.Add(review);
        }

        review.Rating = rating;
        review.Comment = comment;
        review.CreatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        TempData["ReviewMessage"] = "Your review has been saved.";
        return RedirectToAction("Details", "Products", new { id = catalogCardId, area = "" }) is RedirectToActionResult redirect
            ? Redirect(Url.Action("Details", "Products", new { id = catalogCardId }) + "#reviews")
            : Redirect("/Products/Details/" + catalogCardId + "#reviews");
    }
}