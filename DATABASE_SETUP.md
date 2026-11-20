# PokéMarket database setup

## Current DbContexts and databases

The existing solution intentionally has three DbContexts. Keep them separate until a deliberate schema consolidation is designed and tested.

| Project | DbContext | Default SQLite file | Purpose |
|---|---|---|---|
| PokemonMvc | PokemonContext | pokemonmvc.db | Pokémon cards and sealed products |
| PokemonMvc | CatalogueIdentityContext | pokemonmvc-auth.db | Catalogue admin/user Identity |
| PokeMarketApi | MarketContext | pokemarket.db | Marketplace Identity, collections, listings, favourites, reviews, snapshots and messages |

The two applications are separate processes. Their database provider is selected independently in each appsettings file.

## Default local mode (SQLite)

The repository is configured to run locally without a SQL Server installation:

**PokemonMvc/appsettings.json**
- Database.Provider: Sqlite
- ConnectionStrings.PokemonContext: Data Source=pokemonmvc.db
- ConnectionStrings.AccessConnection: Data Source=pokemonmvc-auth.db

**PokeMarketApi/appsettings.json**
- Database.Provider: Sqlite
- ConnectionStrings.MarketDb: Data Source=pokemarket.db

The files are created beside the running application according to the process working directory. Keep backups before changing the schema.

## Switch to SQL Server at school

The SQL Server EF Core provider is included in both project files. When you have the server details from your teacher, set the provider to `SqlServer` in both appsettings files.

Example only — replace `YOUR_SERVER_NAME` with the actual SQL Server instance name:

**PokemonMvc/appsettings.json**
```json
{
  "Database": { "Provider": "SqlServer" },
  "ConnectionStrings": {
    "PokemonContext": "Server=YOUR_SERVER_NAME;Database=PokeMarketCatalogDb;Trusted_Connection=True;TrustServerCertificate=True;",
    "AccessConnection": "Server=YOUR_SERVER_NAME;Database=PokeMarketCatalogueIdentityDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "AllowedHosts": "*"
}
```

**PokeMarketApi/appsettings.json**
```json
{
  "Database": { "Provider": "SqlServer" },
  "ConnectionStrings": {
    "MarketDb": "Server=YOUR_SERVER_NAME;Database=PokeMarketDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "CatalogApi": { "BaseUrl": "http://localhost:5165/" },
  "AllowedHosts": "*"
}
```

Use the actual authentication method required by the school server. If SQL authentication is required, obtain the credentials from your teacher and keep them in Visual Studio user secrets or environment variables. Do not commit them.

## Important schema / migration warning

The current code uses `EnsureCreated()` for its existing local database initialization and contains compatibility SQL for older marketplace SQLite files. `EnsureCreated()` is not a substitute for an EF Core migrations workflow and should not be mixed casually with migrations on the same database.

Before using an existing school database or deploying to production:

1. Back up the database.
2. Confirm the target database names and permissions.
3. Review the current entity models and the schema expected by each DbContext.
4. Generate and review a baseline migration for each DbContext using Visual Studio Package Manager Console.
5. Use the correct project as both the Default project and Startup project for each migration command.
6. Do not run `EnsureCreated()` against a database managed by migrations. Replace the initialization approach with `Database.Migrate()` only after migrations have been generated and reviewed.

### Package Manager Console commands

For the catalogue database context:

```powershell
Add-Migration InitialCatalogueSchema -Project PokemonMvc -StartupProject PokemonMvc -Context PokemonContext
Update-Database -Project PokemonMvc -StartupProject PokemonMvc -Context PokemonContext
```

For the catalogue Identity database context:

```powershell
Add-Migration InitialCatalogueIdentitySchema -Project PokemonMvc -StartupProject PokemonMvc -Context CatalogueIdentityContext
Update-Database -Project PokemonMvc -StartupProject PokemonMvc -Context CatalogueIdentityContext
```

For the marketplace database context:

```powershell
Add-Migration InitialMarketplaceSchema -Project PokeMarketApi -StartupProject PokeMarketApi -Context MarketContext
Update-Database -Project PokeMarketApi -StartupProject PokeMarketApi -Context MarketContext
```

These are schema-generation instructions, not evidence that migrations have already been generated or applied. Review each generated migration before running Update-Database, especially when connecting to a school database with existing data.
