# PokéMarket

A .NET 10 solution containing two separate ASP.NET Core MVC applications.

## Projects

- **PokemonMvc** owns the catalogue SQLite database and exposes read-only JSON endpoints under `/api/catalog`.
- **PokeMarketApi** consumes the catalogue over HTTP and owns its own accounts, portfolio, seller listings and community messages.

## Run both projects in Visual Studio

1. Open `PokeMarket.sln` in Visual Studio 2026.
2. Right-click the solution → **Properties** → **Startup Project**.
3. Choose **Multiple startup projects** and set both `PokemonMvc` and `PokeMarketApi` to **Start**.
4. Press F5. Stable local addresses are:
   - Catalogue: `http://localhost:5165` (HTTPS also available on 7165)
   - Marketplace: `https://localhost:7265` (HTTP also available on 5265)
5. Test the catalogue endpoint at `http://localhost:5165/api/catalog/cards`. The marketplace reads this URL from `PokeMarketApi/appsettings.json`.

## Create the catalogue administrator

Catalogue create/edit/delete pages require the **Admin** role. For local development, set these environment variables in Visual Studio's PokemonMvc launch profile (Project → Properties → Debug → Open debug launch profiles UI):

- `POKEMARKET_ADMIN_EMAIL`: your chosen admin email
- `POKEMARKET_ADMIN_PASSWORD`: a strong password with at least 8 characters and a digit

Restart PokemonMvc after setting them. The app creates that account and assigns the Admin role on startup. Then sign in at `/Identity/Account/Login`. Do not commit real credentials to GitHub.

## Marketplace features

- Browse and search catalogue cards and sets.
- Sign up/sign in with ASP.NET Core Identity.
- Add/remove owned cards from a private portfolio.
- Create community seller listings.
- Browse registered members and exchange private messages.
- Convert QAR display prices using a live exchange-rate endpoint when it is reachable. If the rate service is unavailable, the page remains in QAR.
- Community listings are demonstrations only; no payments or checkout are processed.

## Important demo limitations

- Seed catalogue prices are illustrative sample values, not verified live market quotes.
- Historical price charts and percentage movements are intentionally not fabricated. They remain unavailable until a legitimate price-history data provider is configured.
- This project does not provide payments, escrow, shipping, PSA verification, or settlement.
- The community directory shows registered accounts in this local demo. Do not use real personal information in a public deployment without adding privacy and abuse controls.

## Local database note

Both apps use separate SQLite databases in their project folders. If you already ran an older version of this project before Identity was added, back up any catalogue data before considering a database reset. Do not delete either database if you have manually entered cards you need to keep.
