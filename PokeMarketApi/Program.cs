using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PokeMarketApi.Data;
using PokeMarketApi.Services;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var databaseProvider = builder.Configuration["Database:Provider"] ?? "Sqlite";
var marketConnection = builder.Configuration.GetConnectionString("MarketDb")
    ?? throw new InvalidOperationException("Connection string 'MarketDb' is missing.");

builder.Services.AddDbContext<MarketContext>(options =>
    ConfigureDatabase(options, databaseProvider, marketConnection));

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
}).AddEntityFrameworkStores<MarketContext>();

// Give this app its own cookie names so the two local applications cannot
// overwrite each other's Identity or antiforgery cookies.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = ".PokeMarketApi.Identity";
    options.LoginPath = "/Identity/Account/Login";
});
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name = ".PokeMarketApi.Antiforgery";
});

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

    // Keep older local SQLite databases usable while also supporting SQL Server.
    await EnsureColumn(db, "PortfolioItems", "Quantity", "INTEGER NOT NULL DEFAULT 1", "int NOT NULL DEFAULT 1");
    await EnsureColumn(db, "PortfolioItems", "Condition", "TEXT NOT NULL DEFAULT 'Near Mint'", "nvarchar(40) NOT NULL DEFAULT 'Near Mint'");
    await EnsureColumn(db, "SellerListings", "Condition", "TEXT NOT NULL DEFAULT 'Near Mint'", "nvarchar(40) NOT NULL DEFAULT 'Near Mint'");
    await EnsureColumn(db, "SellerListings", "Status", "TEXT NOT NULL DEFAULT 'Available'", "nvarchar(30) NOT NULL DEFAULT 'Available'");

    if (db.Database.IsSqlite())
    {
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
    else if (db.Database.IsSqlServer())
    {
        db.Database.ExecuteSqlRaw("""
            IF OBJECT_ID(N'dbo.ConversationMessages', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.ConversationMessages (
                    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_ConversationMessages PRIMARY KEY,
                    SenderId nvarchar(450) NOT NULL,
                    RecipientId nvarchar(450) NOT NULL,
                    Message nvarchar(max) NOT NULL,
                    SentAt datetime2 NOT NULL
                );
            END;

            IF OBJECT_ID(N'dbo.FavouriteItems', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.FavouriteItems (
                    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_FavouriteItems PRIMARY KEY,
                    UserId nvarchar(450) NOT NULL,
                    CatalogCardId int NOT NULL,
                    ProductName nvarchar(max) NOT NULL,
                    ImageUrl nvarchar(max) NULL,
                    SetName nvarchar(max) NULL,
                    PriceQar decimal(18,2) NOT NULL
                );
            END;

            IF OBJECT_ID(N'dbo.ProductReviews', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.ProductReviews (
                    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_ProductReviews PRIMARY KEY,
                    CatalogCardId int NOT NULL,
                    UserId nvarchar(450) NOT NULL,
                    UserEmail nvarchar(max) NOT NULL,
                    Rating int NOT NULL,
                    Comment nvarchar(800) NOT NULL,
                    CreatedAt datetime2 NOT NULL
                );
            END;

            IF OBJECT_ID(N'dbo.PriceSnapshots', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.PriceSnapshots (
                    Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_PriceSnapshots PRIMARY KEY,
                    CatalogCardId int NOT NULL,
                    ProductName nvarchar(max) NOT NULL,
                    PriceQar decimal(18,2) NOT NULL,
                    RecordedAt datetime2 NOT NULL,
                    IsDemoData bit NOT NULL CONSTRAINT DF_PriceSnapshots_IsDemoData DEFAULT 1
                );
            END;

            IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ProductReviews_CatalogCardId' AND object_id = OBJECT_ID(N'dbo.ProductReviews'))
                CREATE INDEX IX_ProductReviews_CatalogCardId ON dbo.ProductReviews (CatalogCardId);

            IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PriceSnapshots_CatalogCardId_RecordedAt' AND object_id = OBJECT_ID(N'dbo.PriceSnapshots'))
                CREATE INDEX IX_PriceSnapshots_CatalogCardId_RecordedAt ON dbo.PriceSnapshots (CatalogCardId, RecordedAt);
            """);
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

static void ConfigureDatabase(
    DbContextOptionsBuilder options,
    string provider,
    string connectionString)
{
    if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
    {
        options.UseSqlServer(connectionString);
        return;
    }

    if (provider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
    {
        options.UseSqlite(connectionString);
        return;
    }

    throw new InvalidOperationException(
        $"Unsupported database provider '{provider}'. Use 'Sqlite' or 'SqlServer'.");
}

static async Task EnsureColumn(
    MarketContext db,
    string table,
    string column,
    string sqliteDefinition,
    string sqlServerDefinition)
{
    if (db.Database.IsSqlite())
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
            {
                if (string.Equals(reader.GetString(1), column, StringComparison.OrdinalIgnoreCase))
                    found = true;
            }

            await reader.CloseAsync();
            if (!found)
                await db.Database.ExecuteSqlRawAsync(
                    $"ALTER TABLE \"{table}\" ADD COLUMN \"{column}\" {sqliteDefinition}");
        }
        finally
        {
            if (wasClosed) await connection.CloseAsync();
        }

        return;
    }

    if (db.Database.IsSqlServer())
    {
        var exists = await db.Database.SqlQueryRaw<int>(
            $"SELECT CASE WHEN COL_LENGTH(N'dbo.{table}', N'{column}') IS NULL THEN 0 ELSE 1 END AS [Value]")
            .SingleAsync();

        if (exists == 0)
            await db.Database.ExecuteSqlRawAsync(
                $"ALTER TABLE [dbo].[{table}] ADD [{column}] {sqlServerDefinition}");
    }
}
