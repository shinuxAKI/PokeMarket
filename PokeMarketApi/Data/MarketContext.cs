using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PokeMarketApi.Models;

namespace PokeMarketApi.Data;

public class MarketContext(DbContextOptions<MarketContext> o)
    : IdentityDbContext<IdentityUser>(o)
{
    public DbSet<PortfolioItem> PortfolioItems => Set<PortfolioItem>();
    public DbSet<SellerListing> SellerListings => Set<SellerListing>();
    public DbSet<ConversationMessage> ConversationMessages => Set<ConversationMessage>();
    public DbSet<FavouriteItem> FavouriteItems => Set<FavouriteItem>();
    public DbSet<ProductReview> ProductReviews => Set<ProductReview>();
    public DbSet<PriceSnapshot> PriceSnapshots => Set<PriceSnapshot>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<FavouriteItem>()
            .HasIndex(item => new { item.UserId, item.CatalogCardId })
            .IsUnique();

        builder.Entity<ConversationMessage>()
            .HasIndex(message => new { message.SenderId, message.RecipientId });

        builder.Entity<ProductReview>()
            .HasIndex(review => review.CatalogCardId);

        builder.Entity<PriceSnapshot>()
            .HasIndex(snapshot => new { snapshot.CatalogCardId, snapshot.RecordedAt });
    }
}