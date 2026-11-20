# PokéMarket

PokéMarket is a .NET 10 solution containing two cooperating ASP.NET Core applications. The existing application structure is preserved so current catalogue and marketplace features are not discarded.

## Solution projects

- **PokemonMvc** — catalogue website and catalogue JSON endpoints under `/api/catalog`. It owns the card catalogue and catalogue administrator Identity context.
- **PokeMarketApi** — the customer-facing marketplace web application. It consumes the catalogue through `CatalogClient` and owns marketplace accounts, collections, wishlists, seller listings, reviews, price snapshots and community messages.

> Architecture note: despite its project name, the current `PokeMarketApi` project is an MVC marketplace application, not yet a standalone REST-only API service. The current JSON catalogue endpoints are hosted by `PokemonMvc`. Public developer API-key management remains future work.

## Open and run in Visual Studio

1. Open `PokeMarket.sln` in Visual Studio 2026 with the .NET 10 SDK installed.
2. Right-click the solution → **Properties** → **Startup Project**.
3. Select **Multiple startup projects** and set both `PokemonMvc` and `PokeMarketApi` to **Start**.
4. Configure the SQL Server connection strings in both appsettings files.
5. Generate and apply the three EF Core migrations as described in [DATABASE_SETUP.md](DATABASE_SETUP.md).
6. Press F5.
7. Default application addresses:
   - Catalogue: `http://localhost:5165` (HTTPS: `https://localhost:7165`)
   - Marketplace: `https://localhost:7265` (HTTP: `http://localhost:5265`)
8. Test the catalogue endpoint at `http://localhost:5165/api/catalog/cards`. The marketplace reads this address from `PokeMarketApi/appsettings.json`.

## SQL Server database architecture

**Both projects use Microsoft SQL Server only.** SQLite packages, providers, configuration and SQLite-specific database initialization code have been removed.

The solution currently keeps three DbContexts / physical databases to preserve the existing separation:

| Application | DbContext | SQL Server database |
|---|---|---|
| PokemonMvc | `PokemonContext` | `PokeMarketCatalogDb` |
| PokemonMvc | `CatalogueIdentityContext` | `PokeMarketCatalogueIdentityDb` |
| PokeMarketApi | `MarketContext` | `PokeMarketDb` |

The committed connection strings contain `YOUR_SCHOOL_SQL_SERVER` placeholders. Replace these with the actual SQL Server instance supplied by your teacher. Do not commit real passwords or credentials. See [DATABASE_SETUP.md](DATABASE_SETUP.md) for the migration commands and authentication guidance.

**Migrations must be generated before running the applications for the first time.** Both applications call `Database.Migrate()` at startup; that applies migrations already present in the project, but does not generate them. Use the Package Manager Console commands in DATABASE_SETUP.md for all three contexts.

## Catalogue administrator (development only)

Catalogue create/edit/delete pages and the admin dashboard require the **Admin** role. Set these environment variables in the PokemonMvc launch profile:

- `POKEMARKET_ADMIN_EMAIL`: your chosen admin email
- `POKEMARKET_ADMIN_PASSWORD`: a strong password with at least 8 characters and a digit

Restart PokemonMvc after setting them. The development fallback is `admin@pokedex.com` / `PokedexAdmin123!`. Do not use fallback credentials in production or commit real credentials to GitHub.

## Current features

### PokemonMvc catalogue
- Public catalogue with product search, set filtering, sorting by name, set and price, and pagination.
- Admin-only product create, edit and delete with photo URL fields.
- Admin dashboard with catalogue counts, product mix, estimated catalogue value, top sets and recently added products.

### PokeMarketApi marketplace
- User registration and sign-in.
- Browse, search, filter, sort and paginate catalogue products.
- Private collection tracker with quantity, card condition and PSA grade.
- Wishlist / saved favourites.
- Seller listings with edit, delete, condition, grade and Available/Sold status controls.
- Product star ratings and written reviews.
- Product price-history chart using clearly labelled illustrative demo snapshots.
- Registered-member community and private messages.
- Currency display conversion when the external exchange-rate service is reachable.

## Demo limitations and future work

- Seed catalogue prices are illustrative sample values, not verified live market quotes.
- Price-history snapshots are generated demo values based on catalogue prices, not verified market transactions.
- Community listings are demonstrations only; no payments, checkout, escrow, shipping or settlement are processed.
- PSA grade labels are user-entered information and are not verified by PSA or another grading service.
- Add privacy, moderation and abuse controls before any public deployment.
- A standalone public developer API, API access request workflow, API-key issuance/rotation/revocation and API-key authentication still need implementation.

## Important safety notes

- Never commit production passwords, API keys or private connection strings.
- Back up school databases before schema changes.
- Do not delete or recreate databases as a troubleshooting shortcut.
- Review build and runtime results locally before deploying to IIS.
