using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using PokeMarketApi.Data;
using PokeMarketApi.Services;
using System.Data;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddDataProtection().SetApplicationName("PokeMarketApi");
builder.Services.AddDbContext<MarketContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("MarketDb")));
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
}).AddRoles<IdentityRole>().AddEntityFrameworkStores<MarketContext>();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("PublicApi", limiter =>
    {
        limiter.PermitLimit = 60;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueLimit = 0;
        limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiter.AutoReplenishment = true;
    });
});

// Isolate cookies from the separate catalogue application.
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
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<MarketContext>();
    db.Database.EnsureCreated();

    db.Database.ExecuteSqlRaw("""
        CREATE TABLE IF NOT EXISTS ApiClientRegistrations (
            Id INTEGER NOT NULL CONSTRAINT PK_ApiClientRegistrations PRIMARY KEY AUTOINCREMENT,
            UserId TEXT NOT NULL, ApplicationName TEXT NOT NULL, ContactEmail TEXT NOT NULL,
            Website TEXT NULL, Purpose TEXT NOT NULL, KeyHash TEXT NOT NULL,
            ProtectedKey TEXT NULL, Status TEXT NOT NULL DEFAULT 'Approved', CreatedAt TEXT NOT NULL,
            ReviewedAt TEXT NULL, ReviewedBy TEXT NULL, AdminNotes TEXT NULL
        );
        """);

    await EnsureColumn(db, "PortfolioItems", "Quantity", "INTEGER NOT NULL DEFAULT 1");
    await EnsureColumn(db, "PortfolioItems", "Condition", "TEXT NOT NULL DEFAULT 'Near Mint'");
    await EnsureColumn(db, "SellerListings", "Condition", "TEXT NOT NULL DEFAULT 'Near Mint'");
    await EnsureColumn(db, "SellerListings", "Status", "TEXT NOT NULL DEFAULT 'Available'");
    await EnsureColumn(db, "ApiClientRegistrations", "ProtectedKey", "TEXT NULL");
    // Preserve credentials issued by the earlier version as approved.
    await EnsureColumn(db, "ApiClientRegistrations", "Status", "TEXT NOT NULL DEFAULT 'Approved'");
    await EnsureColumn(db, "ApiClientRegistrations", "ReviewedAt", "TEXT NULL");
    await EnsureColumn(db, "ApiClientRegistrations", "ReviewedBy", "TEXT NULL");
    await EnsureColumn(db, "ApiClientRegistrations", "AdminNotes", "TEXT NULL");

    db.Database.ExecuteSqlRaw("""
        CREATE TABLE IF NOT EXISTS ApiClientRegistrations (
            Id INTEGER NOT NULL CONSTRAINT PK_ApiClientRegistrations PRIMARY KEY AUTOINCREMENT,
            UserId TEXT NOT NULL, ApplicationName TEXT NOT NULL, ContactEmail TEXT NOT NULL,
            Website TEXT NULL, Purpose TEXT NOT NULL, KeyHash TEXT NOT NULL,
            ProtectedKey TEXT NULL, Status TEXT NOT NULL DEFAULT 'Approved', CreatedAt TEXT NOT NULL,
            ReviewedAt TEXT NULL, ReviewedBy TEXT NULL, AdminNotes TEXT NULL
        );
        CREATE UNIQUE INDEX IF NOT EXISTS IX_ApiClientRegistrations_KeyHash ON ApiClientRegistrations (KeyHash);
        CREATE INDEX IF NOT EXISTS IX_ApiClientRegistrations_UserId ON ApiClientRegistrations (UserId);
        """);

    db.Database.ExecuteSqlRaw("""
        CREATE TABLE IF NOT EXISTS ConversationMessages (
            Id INTEGER NOT NULL CONSTRAINT PK_ConversationMessages PRIMARY KEY AUTOINCREMENT,
            SenderId TEXT NOT NULL, RecipientId TEXT NOT NULL, Message TEXT NOT NULL, SentAt TEXT NOT NULL
        );
        CREATE INDEX IF NOT EXISTS IX_ConversationMessages_SenderId_RecipientId ON ConversationMessages (SenderId, RecipientId);
        CREATE TABLE IF NOT EXISTS FavouriteItems (
            Id INTEGER NOT NULL CONSTRAINT PK_FavouriteItems PRIMARY KEY AUTOINCREMENT,
            UserId TEXT NOT NULL, CatalogCardId INTEGER NOT NULL, ProductName TEXT NOT NULL,
            ImageUrl TEXT NULL, SetName TEXT NULL, PriceQar TEXT NOT NULL
        );
        CREATE UNIQUE INDEX IF NOT EXISTS IX_FavouriteItems_UserId_CatalogCardId ON FavouriteItems (UserId, CatalogCardId);
        CREATE TABLE IF NOT EXISTS ProductReviews (
            Id INTEGER NOT NULL CONSTRAINT PK_ProductReviews PRIMARY KEY AUTOINCREMENT,
            CatalogCardId INTEGER NOT NULL, UserId TEXT NOT NULL, UserEmail TEXT NOT NULL,
            Rating INTEGER NOT NULL, Comment TEXT NOT NULL, CreatedAt TEXT NOT NULL
        );
        CREATE INDEX IF NOT EXISTS IX_ProductReviews_CatalogCardId ON ProductReviews (CatalogCardId);
        CREATE TABLE IF NOT EXISTS PriceSnapshots (
            Id INTEGER NOT NULL CONSTRAINT PK_PriceSnapshots PRIMARY KEY AUTOINCREMENT,
            CatalogCardId INTEGER NOT NULL, ProductName TEXT NOT NULL, PriceQar TEXT NOT NULL,
            RecordedAt TEXT NOT NULL, IsDemoData INTEGER NOT NULL DEFAULT 1
        );
        CREATE INDEX IF NOT EXISTS IX_PriceSnapshots_CatalogCardId_RecordedAt ON PriceSnapshots (CatalogCardId, RecordedAt);
        """);

    var adminEmail = builder.Configuration["Admin:Email"];
    var adminPassword = builder.Configuration["Admin:Password"];
    if (!string.IsNullOrWhiteSpace(adminEmail) && !string.IsNullOrWhiteSpace(adminPassword))
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        if (!await roleManager.RoleExistsAsync("Admin"))
            await roleManager.CreateAsync(new IdentityRole("Admin"));

        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin == null)
        {
            admin = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
            var createResult = await userManager.CreateAsync(admin, adminPassword);
            if (!createResult.Succeeded)
                throw new InvalidOperationException("Could not create the API administrator: " + string.Join("; ", createResult.Errors.Select(x => x.Description)));
        }
        if (!await userManager.IsInRoleAsync(admin, "Admin"))
            await userManager.AddToRoleAsync(admin, "Admin");
    }
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
            if (string.Equals(reader.GetString(1), column, StringComparison.OrdinalIgnoreCase)) found = true;
        await reader.CloseAsync();
        if (!found)
            await db.Database.ExecuteSqlRawAsync($"ALTER TABLE \"{table}\" ADD COLUMN \"{column}\" {definition}");
    }
    finally { if (wasClosed) await connection.CloseAsync(); }
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.Run();