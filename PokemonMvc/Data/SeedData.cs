using PokemonMvc.Models;

namespace PokemonMvc.Data;

public static class SeedData
{
    public static void Initialize(PokemonContext db)
    {
        // Seed by card number + set so future sample additions are applied
        // without duplicating catalogue entries on existing installations.
        var samples = new[]
        {
            new Card { Name = "Charizard ex", CardNumber = "223/197", SetName = "Obsidian Flames", Rarity = "Special Illustration Rare", MarketPriceQar = 420m, ImageUrl = "https://images.pokemontcg.io/sv3/223.png", Language = "English" },
            new Card { Name = "Pikachu", CardNumber = "173/165", SetName = "151", Rarity = "Illustration Rare", MarketPriceQar = 95m, ImageUrl = "https://images.pokemontcg.io/sv3pt5/173.png", Language = "English" },
            new Card { Name = "Mew ex", CardNumber = "205/165", SetName = "151", Rarity = "Special Illustration Rare", MarketPriceQar = 210m, ImageUrl = "https://images.pokemontcg.io/sv3pt5/205.png", Language = "English" },
            new Card { Name = "Gengar", CardNumber = "104/162", SetName = "Temporal Forces", Rarity = "Rare", MarketPriceQar = 18m, ImageUrl = "https://images.pokemontcg.io/sv5/104.png", Language = "English" },
            new Card { Name = "Eevee", CardNumber = "133/165", SetName = "151", Rarity = "Common", MarketPriceQar = 3m, ImageUrl = "https://images.pokemontcg.io/sv3pt5/133.png", Language = "English" }
        };

        foreach (var sample in samples)
        {
            var exists = db.Cards.Any(card =>
                card.CardNumber == sample.CardNumber &&
                card.SetName == sample.SetName);

            if (!exists)
            {
                db.Cards.Add(sample);
            }
        }

        db.SaveChanges();
    }
}