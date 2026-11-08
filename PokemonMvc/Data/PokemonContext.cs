using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PokemonMvc.Models;
namespace PokemonMvc.Data;
public class PokemonContext(DbContextOptions<PokemonContext> o) : IdentityDbContext<IdentityUser>(o)
{
    public DbSet<Card> Cards => Set<Card>();
}