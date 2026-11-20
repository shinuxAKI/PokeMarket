using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace PokemonMvc.Data;

// These factories let EF Core scaffold migrations without starting the web app
// or trying to seed/query a database before the initial migrations exist.
public sealed class PokemonContextFactory : IDesignTimeDbContextFactory<PokemonContext>
{
    public PokemonContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();
        var connectionString = configuration.GetConnectionString("PokemonContext")
            ?? throw new InvalidOperationException("Connection string 'PokemonContext' is missing.");

        var options = new DbContextOptionsBuilder<PokemonContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new PokemonContext(options);
    }

    internal static IConfiguration BuildConfiguration() =>
        new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables()
            .Build();
}

public sealed class CatalogueIdentityContextFactory
    : IDesignTimeDbContextFactory<CatalogueIdentityContext>
{
    public CatalogueIdentityContext CreateDbContext(string[] args)
    {
        var configuration = PokemonContextFactory.BuildConfiguration();
        var connectionString = configuration.GetConnectionString("AccessConnection")
            ?? throw new InvalidOperationException("Connection string 'AccessConnection' is missing.");

        var options = new DbContextOptionsBuilder<CatalogueIdentityContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new CatalogueIdentityContext(options);
    }
}
