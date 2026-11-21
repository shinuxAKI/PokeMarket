using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using PokeMarketApi.Data;
using PokeMarketApi.Services;

namespace PokeMarketApi.Controllers;

[ApiController]
[Route("api/v1")]
[EnableRateLimiting("PublicApi")]
public class PublicApiController(MarketContext db, CatalogClient catalog) : ControllerBase
{
    [HttpGet("cards")]
    public async Task<IActionResult> Cards([FromHeader(Name = "X-API-Key")] string? apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            return Unauthorized(new { error = "Provide your API key in the X-API-Key header." });

        var hash = ApiRequestController.HashKey(apiKey);
        var validKey = await db.ApiClientRegistrations.AsNoTracking()
            .AnyAsync(x => x.KeyHash == hash && x.Status == "Approved");

        if (!validKey)
            return Unauthorized(new { error = "The API key is invalid, pending approval, or revoked." });

        try
        {
            return Ok(await catalog.GetCards());
        }
        catch (HttpRequestException)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { error = "The catalogue service is temporarily unavailable." });
        }
        catch (TaskCanceledException)
        {
            return StatusCode(StatusCodes.Status504GatewayTimeout,
                new { error = "The catalogue service did not respond in time." });
        }
    }
}
