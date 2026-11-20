using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PokeMarketApi.Data;
using PokeMarketApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var marketConnection = builder.Configuration.GetConnectionString("MarketDb")
    ?? throw new InvalidOperationException("Connection string 'MarketDb' is missing.");

builder.Services.AddDbContext<MarketContext>(options =>
    options.UseSqlServer(marketConnection));

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
    await db.Database.MigrateAsync();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.Run();