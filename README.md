# PokeMarket

A .NET 10 solution with two independent ASP.NET Core applications:

- **PokemonMvc** — SQLite-backed card catalogue and admin CRUD. Publishes read-only JSON catalogue endpoints.
- **PokeMarketApi** — white-and-blue Pokémon TCG marketplace UI. Reads the MVC catalogue over HTTP and stores demo marketplace accounts/listings in its own JSON store. It never connects to the MVC database.

## Requirements

- .NET 10 SDK
- Visual Studio 2022 (17.14+) with ASP.NET and web development workload

## Run locally

Open `PokeMarket.sln` in Visual Studio. Set both projects to Multiple Startup Projects (PokemonMvc and PokeMarketApi) or start the projects one at a time.

Default local URLs:
- PokemonMvc: https://localhost:7210 (HTTP: http://localhost:5210)
- PokeMarketApi: https://localhost:7165 (HTTP: http://localhost:5165)

The marketplace reads `CatalogApi:BaseUrl` from appsettings.json. Keep it pointed to the MVC URL that you are running.

## First-run demo admin

- Email: `admin@pokemarket.local`
- Password: `ChangeMe123!`

Change these values in `PokemonMvc/appsettings.json` before using the application beyond a local demonstration. The admin user is seeded into the local SQLite database.

## External data

- Catalogue seed cards use public images from the Pokémon TCG API image CDN; users can upload their own photos in the MVC admin.
- Daily price-history integration uses the PkmnPrices API. It is optional and requires a key from https://www.pkmnprices.com/. Configure `PkmnPrices:ApiKey` using .NET user-secrets or an environment variable. Without a key, the UI clearly says live history is not configured rather than inventing chart data.
- Currency conversion uses the open.er-api.com daily-rate endpoint and displays attribution.

## Important demo limitations

The API's account, listing, and message store is a JSON-file demonstration store, not a production transactional database. Passwords are hashed. No real card payment is processed. Seller listing and portfolio actions are demonstrations and should not be represented as completed cash sales. Before public production use, add a reviewed persistent backend, email verification, abuse controls, and a payment provider.

See [SETUP.md](SETUP.md) for complete setup steps.
