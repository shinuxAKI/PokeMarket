using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PokeMarketApi.Data;
using PokeMarketApi.Models;

namespace PokeMarketApi.Controllers;

[Authorize]
public class SocialController(MarketContext db, UserManager<IdentityUser> users) : Controller
{
    public async Task<IActionResult> Index()
    {
        var currentId = users.GetUserId(User);
        var people = await users.Users.Where(x => x.Id != currentId).OrderBy(x => x.Email).ToListAsync();
        return View(people);
    }

    public async Task<IActionResult> Chat(string userId)
    {
        var currentId = users.GetUserId(User)!;
        var contact = await users.FindByIdAsync(userId);
        if (contact == null || contact.Id == currentId) return NotFound();
        ViewBag.Contact = contact;
        ViewBag.ContactId = contact.Id;
        var messages = await db.ConversationMessages
            .Where(x => (x.SenderId == currentId && x.RecipientId == userId) ||
                        (x.SenderId == userId && x.RecipientId == currentId))
            .OrderBy(x => x.SentAt).ToListAsync();
        return View(messages);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(string recipientId, string message)
    {
        var text = (message ?? "").Trim();
        var currentId = users.GetUserId(User)!;
        var recipient = await users.FindByIdAsync(recipientId);
        if (recipient == null || recipient.Id == currentId || text.Length == 0 || text.Length > 1000)
            return BadRequest("Message or recipient is invalid.");
        db.ConversationMessages.Add(new ConversationMessage
        {
            SenderId = currentId, RecipientId = recipientId, Message = text
        });
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Chat), new { userId = recipientId });
    }
}