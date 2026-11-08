using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PokeMarketApi.Data;
using PokeMarketApi.Services;
using System.Data;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddDbContext<MarketContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("MarketDb")));
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
}).AddEntityFrameworkStores<MarketContext>();
builder.Services.AddHttpClient<CatalogClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["CatalogApi:BaseUrl"] ?? "http://localhost:5165/");
    client.Timeout = TimeSpan.FromSeconds(8);
});

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MarketContext>();
    db.Database.EnsureCreated();

    // EnsureCreated does not add new columns to an existing local SQLite database.
    await EnsureColumn(db, "PortfolioItems", "Quantity", "INTEGER NOT NULL DEFAULT 1");
    await EnsureColumn(db, "PortfolioItems", "Condition", "TEXT NOT NULL DEFAULT 'Near Mint'");
    await EnsureColumn(db, "SellerListings", "Condition", "TEXT NOT NULL DEFAULT 'Near Mint'");
    await EnsureColumn(db, "SellerListings", "Status", "TEXT NOT NULL DEFAULT 'Available'");

    db.Database.ExecuteSqlRaw("""
        CREATE TABLE IF NOT EXISTS ConversationMessages (
            Id INTEGER NOT NULL CONSTRAINT PK_ConversationMessages PRIMARY KEY AUTOINCREMENT,
            SenderId TEXT NOT NULL, RecipientId TEXT NOT NULL, Message TEXT NOT NULL, SentAt TEXT NOT NULL
        );
        CREATE INDEX IF NOT EXISTS IX_ConversationMessages_SenderId_RecipientId
        ON ConversationMessages (SenderId, RecipientId);
        CREATE TABLE IF NOT EXISTS FavouriteItems (
            Id INTEGER NOT NULL CONSTRAINT PK_FavouriteItems PRIMARY KEY AUTOINCREMENT,
            UserId TEXT NOT NULL, CatalogCardId INTEGER NOT NULL, ProductName TEXT NOT NULL,
            ImageUrl TEXT NULL, SetName TEXT NULL, PriceQar TEXT NOT NULL
        );
        CREATE UNIQUE INDEX IF NOT EXISTS IX_FavouriteItems_UserId_CatalogCardId
        ON FavouriteItems (UserId, CatalogCardId);
        CREATE TABLE IF NOT EXISTS ProductReviews (
            Id INTEGER NOT NULL CONSTRAINT PK_ProductReviews PRIMARY KEY AUTOINCREMENT,
            CatalogCardId INTEGER NOT NULL, UserId TEXT NOT NULL, UserEmail TEXT NOT NULL,
            Rating INTEGER NOT NULL, Comment TEXT NOT NULL, CreatedAt TEXT NOT NULL
        );
        CREATE INDEX IF NOT EXISTS IX_ProductReviews_CatalogCardId
        ON ProductReviews (CatalogCardId);
        CREATE TABLE IF NOT EXISTS PriceSnapshots (
            Id INTEGER NOT NULL CONSTRAINT PK_PriceSnapshots PRIMARY KEY AUTOINCREMENT,
            CatalogCardId INTEGER NOT NULL, ProductName TEXT NOT NULL,
            PriceQar TEXT NOT NULL, RecordedAt TEXT NOT NULL, IsDemoData INTEGER NOT NULL DEFAULT 1
        );
        CREATE INDEX IF NOT EXISTS IX_PriceSnapshots_CatalogCardId_RecordedAt
        ON PriceSnapshots (CatalogCardId, RecordedAt);
        """);
}

static async Task EnsureColumn(MarketContext db, string table, string column, string definition)
{
    var connection = db.Database.GetDbConnection();
    var wasClosed = connection.State == ConnectionState.Closed;
    if (wasClosed) await connection.OpenAsync();
    try
    {
        await using var command = connection.CreateCommand();
        command.CommandText = $"PRAGMA table_info(\"{table}\")";
        await using var reader = await command.ExecuteReaderAsync();
        var found = false;
        while (await reader.ReadAsync())
            if (string.Equals(reader.GetString(1), column, StringComparison.OrdinalIgnoreCase))
                found = true;
        await reader.CloseAsync();
        if (!found)
            await db.Database.ExecuteSqlRawAsync($"ALTER TABLE \"{table}\" ADD COLUMN \"{column}\" {definition}");
    }
    finally
    {
        if (wasClosed) await connection.CloseAsync();
    }
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.Run();