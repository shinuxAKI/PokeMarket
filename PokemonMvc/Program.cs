using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PokemonMvc.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var databaseProvider = builder.Configuration["Database:Provider"] ?? "Sqlite";
var catalogueConnection = builder.Configuration.GetConnectionString("PokemonContext")
    ?? throw new InvalidOperationException("Connection string 'PokemonContext' is missing.");
var identityConnection = builder.Configuration.GetConnectionString("AccessConnection")
    ?? throw new InvalidOperationException("Connection string 'AccessConnection' is missing.");

builder.Services.AddDbContext<PokemonContext>(options =>
    ConfigureDatabase(options, databaseProvider, catalogueConnection));

builder.Services.AddDbContext<CatalogueIdentityContext>(options =>
    ConfigureDatabase(options, databaseProvider, identityConnection));

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<CatalogueIdentityContext>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var cardsDb = services.GetRequiredService<PokemonContext>();
    cardsDb.Database.EnsureCreated();
    SeedData.Initialize(cardsDb);

    var identityDb = services.GetRequiredService<CatalogueIdentityContext>();
    identityDb.Database.EnsureCreated();

    // Development-only fallback credentials make a fresh local checkout usable.
    // Environment variables override these values when configured.
    if (app.Environment.IsDevelopment())
    {
        var email = Environment.GetEnvironmentVariable("POKEMARKET_ADMIN_EMAIL")
            ?? "admin@pokedex.com";
        var password = Environment.GetEnvironmentVariable("POKEMARKET_ADMIN_PASSWORD")
            ?? "PokedexAdmin123!";

        var roles = services.GetRequiredService<RoleManager<IdentityRole>>();
        var users = services.GetRequiredService<UserManager<IdentityUser>>();

        if (!await roles.RoleExistsAsync("Admin"))
            await roles.CreateAsync(new IdentityRole("Admin"));

        var admin = await users.FindByEmailAsync(email);
        if (admin == null)
        {
            admin = new IdentityUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var result = await users.CreateAsync(admin, password);
            if (result.Succeeded)
                await users.AddToRoleAsync(admin, "Admin");
        }
        else if (!await users.IsInRoleAsync(admin, "Admin"))
        {
            await users.AddToRoleAsync(admin, "Admin");
        }
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.MapControllers();
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
