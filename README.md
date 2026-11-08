# PokéMarket

A .NET 10 solution containing two separate ASP.NET Core MVC applications.

## Projects

- **PokemonMvc** owns the catalogue SQLite database, public catalogue and admin management dashboard. It exposes read-only JSON endpoints under `/api/catalog`.
- **PokeMarketApi** consumes the catalogue over HTTP and owns marketplace accounts, collections, wishlist, seller listings, reviews and community messages.

## Run both projects in Visual Studio

1. Open `PokeMarket.sln` in Visual Studio 2026.
2. Right-click the solution → **Properties** → **Startup Project**.
3. Choose **Multiple startup projects** and set both `PokemonMvc` and `PokeMarketApi` to **Start**.
4. Press F5. Stable local addresses are:
   - Catalogue: `http://localhost:5165` (HTTPS also available on 7165)
   - Marketplace: `https://localhost:7265` (HTTP also available on 5265)
5. Test the catalogue endpoint at `http://localhost:5165/api/catalog/cards`. The marketplace reads this URL from `PokeMarketApi/appsettings.json`.

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

## Local database note

PokemonMvc stores catalogue cards in `pokemonmvc.db` and admin accounts in `pokemonmvc-auth.db`. PokeMarketApi stores marketplace accounts, portfolio items, seller listings, favourites, reviews, price snapshots and messages in `pokemarket.db`. The two apps do not share a database. Startup compatibility checks add new marketplace columns/tables to older local SQLite databases. Back up your databases before manual schema changes or resets; do not delete them if you have data you need.
