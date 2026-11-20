# PokéMarket setup guide

## Requirements

- Visual Studio 2026 (ASP.NET and web development workload)
- .NET 10 SDK
- NuGet package restore access
- SQLite for the default local setup
- SQL Server / SQL Server LocalDB only when switching to the SQL Server provider

## 1. Get the solution

1. Download the repository ZIP from GitHub or clone the repository.
2. Extract it to a normal writable folder (not inside a protected Windows directory).
3. Open `PokeMarket.sln` in Visual Studio.
4. Allow NuGet restore to finish.

## 2. Configure startup projects

1. Right-click the solution.
2. Select **Properties**.
3. Select **Startup Project**.
4. Choose **Multiple startup projects**.
5. Set `PokemonMvc` and `PokeMarketApi` to **Start**.
6. Keep the project order as PokemonMvc first, PokeMarketApi second.
7. Press F5.

Expected local addresses:

- Catalogue: `http://localhost:5165`
- Marketplace: `https://localhost:7265`

The marketplace's catalogue client uses `CatalogApi:BaseUrl` in `PokeMarketApi/appsettings.json`. If you change the catalogue port, update this setting too.

## 3. Local administrator account

In Visual Studio, open the PokemonMvc project's debug/launch settings and add environment variables:

- `POKEMARKET_ADMIN_EMAIL`
- `POKEMARKET_ADMIN_PASSWORD`

Use a unique development password. The code contains a fallback development account for a fresh local checkout; do not use that fallback in production.

## 4. Switch to SQL Server later

The repository defaults to SQLite so it can run without your school's SQL Server. When you are at school:

1. Back up all existing local `.db` files.
2. Read [DATABASE_SETUP.md](DATABASE_SETUP.md).
3. Change `Database:Provider` to `SqlServer` in both applications.
4. Replace the SQL Server connection-string placeholders with the actual server name and database names supplied by your teacher.
5. Keep each DbContext on its own database unless the schema is deliberately consolidated.
6. Restore NuGet packages and build both projects.
7. Create the SQL Server databases and apply the schema using the instructions in DATABASE_SETUP.md.

Do not copy a real school password into a committed appsettings file. Use Visual Studio user secrets or environment variables for credentials.

## 5. Build verification

In Visual Studio:

1. Select **Build → Restore NuGet Packages**.
2. Select **Build → Rebuild Solution**.
3. Confirm both projects build without errors.
4. Start both projects.
5. Test `http://localhost:5165/api/catalog/cards`.
6. Open the marketplace and verify catalogue cards load.
7. Test registration/login and the marketplace features you use.

A successful build does not by itself prove SQL Server connectivity or IIS deployment. Those must be tested against the actual target environment.

## 6. IIS deployment

Publish each application separately. Configure the production database provider and connection strings outside source control. Install the matching ASP.NET Core Hosting Bundle on the IIS server, grant the application pool identity the required database/file permissions, and verify stdout/Event Viewer logs if startup fails.

Do not publish the development fallback administrator password.
