using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PokeMarketApi.Data;
using PokeMarketApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddDbContext<MarketContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("MarketDb")));

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
}).AddEntityFrameworkStores<MarketContext>();

// Keep this app's authentication cookies separate from the catalogue MVC app.
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
    client.BaseAddress = new Uri(
        builder.Configuration["CatalogApi:BaseUrl"] ?? "http://localhost:5165/");
    client.Timeout = TimeSpan.FromSeconds(8);
});

var app = builder.Build();

// Create the SQL Server schema on first run. The configured SQL Server must be
// reachable before starting the app. The MVC catalogue and Identity schemas
// are initialized independently by the MVC application.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MarketContext>();
    await db.Database.EnsureCreatedAsync();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();