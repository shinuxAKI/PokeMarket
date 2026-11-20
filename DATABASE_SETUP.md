# PokéMarket SQL Server setup

PokéMarket is configured for **Microsoft SQL Server only**. SQLite packages, providers, connection strings and SQLite-specific initialization code have been removed from both applications.

## SQL Server databases and DbContexts

The current solution deliberately keeps its three existing DbContexts separate:

| Project | DbContext | SQL Server database | Purpose |
|---|---|---|---|
| PokemonMvc | PokemonContext | PokeMarketCatalogDb | Pokémon cards and sealed products |
| PokemonMvc | CatalogueIdentityContext | PokeMarketCatalogueIdentityDb | Catalogue admin/user Identity |
| PokeMarketApi | MarketContext | PokeMarketDb | Marketplace Identity, collections, listings, favourites, reviews, snapshots and messages |

Do not point multiple contexts at one database unless their schemas are deliberately consolidated and reviewed.

## Configure the school SQL Server

The committed appsettings files contain placeholders. Replace `YOUR_SCHOOL_SQL_SERVER` with the server/instance name supplied by your teacher.

**PokemonMvc/appsettings.json**
- `ConnectionStrings:PokemonContext`
- `ConnectionStrings:AccessConnection`

**PokeMarketApi/appsettings.json**
- `ConnectionStrings:MarketDb`

The examples currently use Windows Integrated Authentication:

```text
Server=YOUR_SCHOOL_SQL_SERVER;Database=PokeMarketCatalogDb;Trusted_Connection=True;TrustServerCertificate=True;
```

If the school uses SQL Server authentication, ask your teacher for the required authentication method and store the username/password in Visual Studio user secrets or environment variables. Never commit real credentials to GitHub.

The application now expects SQL Server to be reachable at startup. It will not run against the previous local SQLite files.

## Generate migrations in Visual Studio

**Important:** migrations have not been generated in this repository yet. Generate and review them from the current entity models before running either website against the school databases.

1. Open `PokeMarket.sln` in Visual Studio.
2. Restore NuGet packages.
3. Open **Tools → NuGet Package Manager → Package Manager Console**.
4. Set the Default project as shown for each command.
5. Run the commands below one context at a time.

### 1. Catalogue cards

Default project: `PokemonMvc`

```powershell
Add-Migration InitialCatalogueSchema -Project PokemonMvc -StartupProject PokemonMvc -Context PokemonContext
Update-Database -Project PokemonMvc -StartupProject PokemonMvc -Context PokemonContext
```

### 2. Catalogue Identity

Default project: `PokemonMvc`

```powershell
Add-Migration InitialCatalogueIdentitySchema -Project PokemonMvc -StartupProject PokemonMvc -Context CatalogueIdentityContext
Update-Database -Project PokemonMvc -StartupProject PokemonMvc -Context CatalogueIdentityContext
```

### 3. Marketplace and marketplace Identity

Default project: `PokeMarketApi`

```powershell
Add-Migration InitialMarketplaceSchema -Project PokeMarketApi -StartupProject PokeMarketApi -Context MarketContext
Update-Database -Project PokeMarketApi -StartupProject PokeMarketApi -Context MarketContext
```

The `Update-Database` commands create/update the configured databases and tables. Confirm that the connection strings point to the intended school databases before running them. Back up any database that already contains data.

## Startup behavior

Both applications call `Database.Migrate()` at startup. This applies migrations that have already been generated and included in the project. It does **not** create the required schema if no migrations exist, so generate the three initial migrations above before launching the websites.

The catalogue seed routine runs after the catalogue migration. The development admin account is created only in Development and can be configured using `POKEMARKET_ADMIN_EMAIL` and `POKEMARKET_ADMIN_PASSWORD`.

## Build and verify

1. Replace the server placeholders with the real school SQL Server instance.
2. Confirm the Windows account running Visual Studio/IIS has permission to connect and create/update the target databases, or ask your teacher to create the databases and grant permissions.
3. Generate and apply all three migrations.
4. Rebuild the solution.
5. Start both applications.
6. Test `http://localhost:5165/api/catalog/cards` and confirm the marketplace can retrieve catalogue data.

A successful local build does not prove that the school SQL Server is reachable. Test connectivity on the school network with the actual server details.

## IIS deployment

Use the same SQL Server connection strings through environment variables or deployment configuration, not committed source files. Ensure the IIS application-pool identity has the database permissions required by the school's deployment policy.