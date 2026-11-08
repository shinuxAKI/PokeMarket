using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PokeMarketApi.Models;
namespace PokeMarketApi.Data;
public class MarketContext(DbContextOptions<MarketContext> o) : IdentityDbContext<IdentityUser>(o)
{
    public DbSet<PortfolioItem> PortfolioItems => Set<PortfolioItem>();
    public DbSet<SellerListing> SellerListings => Set<SellerListing>();
    public DbSet<ConversationMessage> ConversationMessages => Set<ConversationMessage>();
    public DbSet<FavouriteItem> FavouriteItems => Set<FavouriteItem>();
}