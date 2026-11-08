using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace PokemonMvc.Data;

public class CatalogueIdentityContext(DbContextOptions<CatalogueIdentityContext> options)
    : IdentityDbContext<IdentityUser>(options)
{
}