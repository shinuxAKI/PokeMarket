using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace PokeMarketApi.Data;

// Lets EF Core scaffold migrations without starting the marketplace web app.
public sealed class MarketContextFactory : IDesignTimeDbContextFactory<MarketContext>
{
    public MarketContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("MarketDb")
            ?? throw new InvalidOperationException("Connection string 'MarketDb' is missing.");

        var options = new DbContextOptionsBuilder<MarketContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new MarketContext(options);
    }
}
