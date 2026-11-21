using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PokeMarketApi.Data;
using PokeMarketApi.Services;

namespace PokeMarketApi.Controllers;

[ApiController]
[Route("api/v1")]
public class PublicApiController(MarketContext db, CatalogClient catalog) : ControllerBase
{
    [HttpGet("cards")]
    public async Task<IActionResult> Cards([FromHeader(Name = "X-API-Key")] string? apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            return Unauthorized(new { error = "Provide your API key in the X-API-Key header." });

        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(apiKey)));
        var validKey = await db.ApiClientRegistrations.AsNoTracking()
            .AnyAsync(x => x.KeyHash == hash);

        if (!validKey)
            return Unauthorized(new { error = "The API key is invalid." });

        return Ok(await catalog.GetCards());
    }
}
