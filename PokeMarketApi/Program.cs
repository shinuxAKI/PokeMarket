using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PokeMarketApi.Data;
using PokeMarketApi.Services;

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
    // EnsureCreated does not add tables to an existing SQLite database; this safely adds the messaging table.
    db.Database.ExecuteSqlRaw("""
        CREATE TABLE IF NOT EXISTS ConversationMessages (
            Id INTEGER NOT NULL CONSTRAINT PK_ConversationMessages PRIMARY KEY AUTOINCREMENT,
            SenderId TEXT NOT NULL,
            RecipientId TEXT NOT NULL,
            Message TEXT NOT NULL,
            SentAt TEXT NOT NULL
        );
        CREATE INDEX IF NOT EXISTS IX_ConversationMessages_SenderId_RecipientId
        ON ConversationMessages (SenderId, RecipientId);
        CREATE TABLE IF NOT EXISTS FavouriteItems (
            Id INTEGER NOT NULL CONSTRAINT PK_FavouriteItems PRIMARY KEY AUTOINCREMENT,
            UserId TEXT NOT NULL,
            CatalogCardId INTEGER NOT NULL,
            ProductName TEXT NOT NULL,
            ImageUrl TEXT NULL,
            SetName TEXT NULL,
            PriceQar TEXT NOT NULL
        );
        CREATE UNIQUE INDEX IF NOT EXISTS IX_FavouriteItems_UserId_CatalogCardId
        ON FavouriteItems (UserId, CatalogCardId);
        """);
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.Run();