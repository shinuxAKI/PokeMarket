# PokéMarket setup guide

## Requirements

- Visual Studio 2026 (ASP.NET and web development workload)
- .NET 10 SDK
- NuGet package restore access
- Access to the SQL Server instance used by your school
- Permission to create/update the three project databases, or have your teacher create them and grant the required permissions

## 1. Get the solution

1. Download the repository ZIP from GitHub or clone the repository.
2. Extract it to a normal writable folder (not inside a protected Windows directory).
3. Open `PokeMarket.sln` in Visual Studio.
4. Allow NuGet restore to finish.

## 2. Configure SQL Server

The solution is SQL Server-only. SQLite has been removed.

1. Ask your teacher for the SQL Server instance name and the required authentication method.
2. Open `PokemonMvc/appsettings.json` and replace `YOUR_SCHOOL_SQL_SERVER` in both connection strings.
3. Open `PokeMarketApi/appsettings.json` and replace `YOUR_SCHOOL_SQL_SERVER` in its connection string.
4. The current setup uses Windows Integrated Authentication (`Trusted_Connection=True`). If your school uses SQL authentication, store the supplied credentials in Visual Studio user secrets or environment variables instead of committing them to GitHub.
5. Keep the three DbContexts on their separate databases unless a deliberate schema consolidation is designed.

See [DATABASE_SETUP.md](DATABASE_SETUP.md) for the database names and complete connection-string guidance.

## 3. Generate and apply the database migrations

**Do this before starting the applications.** The repository contains the EF Core SQL Server provider and design-time factories, but the initial migration files must be generated in Visual Studio from the current model classes.

Open **Tools → NuGet Package Manager → Package Manager Console** and run all three migration pairs from [DATABASE_SETUP.md](DATABASE_SETUP.md):

- `PokemonContext` in `PokemonMvc`
- `CatalogueIdentityContext` in `PokemonMvc`
- `MarketContext` in `PokeMarketApi`

Review the generated migration files, verify the connection strings, then run the matching `Update-Database` commands. Never run these commands against a shared school database until your teacher has confirmed the target database and your permissions.

## 4. Configure startup projects

1. Right-click the solution.
2. Select **Properties**.
3. Select **Startup Project**.
4. Choose **Multiple startup projects**.
5. Set `PokemonMvc` and `PokeMarketApi` to **Start**.
6. Keep the project order as PokemonMvc first, PokeMarketApi second.
7. Press F5.

Expected application addresses:

- Catalogue: `http://localhost:5165`
- Marketplace: `https://localhost:7265`

The marketplace's catalogue client uses `CatalogApi:BaseUrl` in `PokeMarketApi/appsettings.json`. If you change the catalogue port, update this setting too.

## 5. Local administrator account

In Visual Studio, open the PokemonMvc project's debug/launch settings and add environment variables:

- `POKEMARKET_ADMIN_EMAIL`
- `POKEMARKET_ADMIN_PASSWORD`

Use a unique development password. The code contains a fallback development account for a fresh development database; do not use that fallback in production.

## 6. Build and verify

In Visual Studio:

1. Select **Build → Restore NuGet Packages**.
2. Select **Build → Rebuild Solution**.
3. Confirm both projects build without errors.
4. Confirm the three migrations have been applied to the intended SQL Server databases.
5. Start both projects.
6. Test `http://localhost:5165/api/catalog/cards`.
7. Open the marketplace and verify catalogue cards load.
8. Test registration/login and the marketplace features you use.

A successful local build does not by itself prove that the school SQL Server is reachable. Connectivity must be tested on the school network with the actual server details.

## 7. IIS deployment

Publish each application separately. Configure the production SQL Server connection strings outside source control. Install the matching ASP.NET Core Hosting Bundle on the IIS server, grant the application pool identity the database permissions required by the school's deployment policy, and verify stdout/Event Viewer logs if startup fails.

Do not publish the development fallback administrator password.
