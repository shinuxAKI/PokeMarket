# PokéMarket

A .NET 10 solution containing two separate ASP.NET Core MVC applications.

## Projects

- **PokemonMvc** owns the catalogue database, public catalogue and admin management dashboard. It exposes read-only JSON endpoints under `/api/catalog`.
- **PokeMarketApi** consumes the catalogue over HTTP and owns marketplace accounts, collections, wishlist, seller listings, reviews and community messages.
- Each application uses its own EF Core DbContext and its own SQL Server database. The catalogue Identity store is also kept separate from the catalogue-card store.

## Database provider

Both projects use **Entity Framework Core 10 with the SQL Server provider**. SQLite packages, SQLite connection strings, SQLite-specific PRAGMA statements and SQLite-specific schema-upgrade SQL have been removed.

The connection strings currently contain the placeholder server name `YOUR_SQL_SERVER`. Replace that value with the SQL Server instance provided by your school when it is available:

| Application / context | Database |
|---|---|
| PokemonMvc / `PokemonContext` | `PokemonMvcDb` |
| PokemonMvc / `CatalogueIdentityContext` | `PokeMarketAccessDb` |
| PokeMarketApi / `MarketContext` | `PokeMarketDb` |

The connection strings are in each project's `appsettings.json`. For a school SQL Server using Windows authentication, the expected format is:

```
Server=YOUR_SQL_SERVER;Database=YOUR_DATABASE;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True
```

If the school provides SQL authentication instead, use the credentials supplied by the school through User Secrets or environment-specific configuration. Do not commit real usernames or passwords to GitHub.

The applications currently call EF Core `EnsureCreated` on startup to create each database schema when the configured database is empty. This is compatible with SQL Server and does not require a live database during compilation. It is **not** an EF Core migrations workflow: do not mix `EnsureCreated` with `Migrate`. A formal migrations baseline should be generated for each DbContext once the final SQL Server schema and school connection details are confirmed.

## Run both projects in Visual Studio

1. Open `PokeMarket.sln` in Visual Studio 2026.
2. Replace the placeholder SQL Server instance in both `appsettings.json` files, or provide the connection strings using environment-specific configuration.
3. Ensure the SQL Server instance is reachable and the Windows account / SQL login has permission to create or access the three databases.
4. Right-click the solution → **Properties** → **Startup Project**.
5. Choose **Multiple startup projects** and set both `PokemonMvc` and `PokeMarketApi` to **Start**.
6. Press F5. Stable local addresses are:
   - Catalogue: `http://localhost:5165` (HTTPS also available on 7165)
   - Marketplace: `https://localhost:7265` (HTTP also available on 5265)
7. Test the catalogue endpoint at `http://localhost:5165/api/catalog/cards`. The marketplace reads this URL from `PokeMarketApi/appsettings.json`.

## Create the catalogue administrator

Catalogue create/edit/delete pages and the admin dashboard require the **Admin** role. For local development, set these environment variables in Visual Studio's PokemonMvc launch profile:

- `POKEMARKET_ADMIN_EMAIL`: your chosen admin email
- `POKEMARKET_ADMIN_PASSWORD`: a strong password with at least 8 characters and a digit

Restart PokemonMvc after setting them. The app creates that account and assigns the Admin role on startup. The development fallback is `admin@pokedex.com` / `PokedexAdmin123!`. Do not use fallback credentials in production or commit real credentials to GitHub.

## Features

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
- Product star ratings and written reviews; one review per signed-in account per product, editable by posting again.
- Product price-history chart using clearly labelled illustrative demo snapshots.
- Registered-member community and private messages.
- Currency display conversion when the external exchange-rate service is reachable.

## Demo limitations

- Seed catalogue prices are illustrative sample values, not verified live market quotes.
- Price-history snapshots are generated demo values based on catalogue prices, not verified market transactions.
- Community listings are demonstrations only; no payments, checkout, escrow, shipping or settlement are processed.
- PSA grade labels are user-entered information and are not verified by PSA or another grading service.
- The community directory uses local demo accounts. Add privacy, moderation and abuse controls before any public deployment.
