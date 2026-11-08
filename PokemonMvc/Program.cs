using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PokemonMvc.Data;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddDbContext<PokemonContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("PokemonContext")));
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
}).AddRoles<IdentityRole>().AddEntityFrameworkStores<PokemonContext>();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<PokemonContext>();
    db.Database.EnsureCreated();
    SeedData.Initialize(db);

    // Create the catalogue administrator only when credentials are supplied by the developer.
    var email = Environment.GetEnvironmentVariable("POKEMARKET_ADMIN_EMAIL");
    var password = Environment.GetEnvironmentVariable("POKEMARKET_ADMIN_PASSWORD");
    if (!string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password))
    {
        var roles = services.GetRequiredService<RoleManager<IdentityRole>>();
        var users = services.GetRequiredService<UserManager<IdentityUser>>();
        if (!await roles.RoleExistsAsync("Admin"))
            await roles.CreateAsync(new IdentityRole("Admin"));
        var admin = await users.FindByEmailAsync(email);
        if (admin == null)
        {
            admin = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
            var result = await users.CreateAsync(admin, password);
            if (result.Succeeded) await users.AddToRoleAsync(admin, "Admin");
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