using PokemonMvc.Models;
namespace PokemonMvc.Data;

public static class SeedData
{
    // These are clearly demo catalogue entries and example QAR prices, not live market quotes.
    public static void Initialize(PokemonContext db)
    {
        var demoCards = new[]
        {
            new Card { Name="Charizard ex", CardNumber="223/197", SetName="Obsidian Flames", Rarity="Special Illustration Rare", MarketPriceQar=420m, ImageUrl="https://images.pokemontcg.io/sv3/223.png" },
            new Card { Name="Pikachu", CardNumber="173/165", SetName="151", Rarity="Illustration Rare", MarketPriceQar=95m, ImageUrl="https://images.pokemontcg.io/sv3pt5/173.png" },
            new Card { Name="Mew ex", CardNumber="205/165", SetName="151", Rarity="Special Illustration Rare", MarketPriceQar=210m, ImageUrl="https://images.pokemontcg.io/sv3pt5/205.png" },
            new Card { Name="Gengar", CardNumber="104/162", SetName="Temporal Forces", Rarity="Rare", MarketPriceQar=18m, ImageUrl="https://images.pokemontcg.io/sv5/104.png" },
            new Card { Name="Eevee", CardNumber="133/165", SetName="151", Rarity="Common", MarketPriceQar=3m, ImageUrl="https://images.pokemontcg.io/sv3pt5/133.png" },
            new Card { Name="Miraidon ex", CardNumber="244/198", SetName="Scarlet & Violet", Rarity="Special Illustration Rare", MarketPriceQar=185m, ImageUrl="https://images.pokemontcg.io/sv1/244.png" },
            new Card { Name="Gardevoir ex", CardNumber="245/198", SetName="Scarlet & Violet", Rarity="Special Illustration Rare", MarketPriceQar=160m, ImageUrl="https://images.pokemontcg.io/sv1/245.png" },
            new Card { Name="Magikarp", CardNumber="203/193", SetName="Paldea Evolved", Rarity="Illustration Rare", MarketPriceQar=240m, ImageUrl="https://images.pokemontcg.io/sv2/203.png" },
            new Card { Name="Iono", CardNumber="269/193", SetName="Paldea Evolved", Rarity="Special Illustration Rare", MarketPriceQar=310m, ImageUrl="https://images.pokemontcg.io/sv2/269.png" },
            new Card { Name="Tyranitar ex", CardNumber="211/197", SetName="Obsidian Flames", Rarity="Special Illustration Rare", MarketPriceQar=75m, ImageUrl="https://images.pokemontcg.io/sv3/211.png" },
            new Card { Name="Pidgeot ex", CardNumber="225/197", SetName="Obsidian Flames", Rarity="Special Illustration Rare", MarketPriceQar=65m, ImageUrl="https://images.pokemontcg.io/sv3/225.png" },
            new Card { Name="Roaring Moon ex", CardNumber="251/182", SetName="Paradox Rift", Rarity="Special Illustration Rare", MarketPriceQar=230m, ImageUrl="https://images.pokemontcg.io/sv4/251.png" },
            new Card { Name="Groudon", CardNumber="199/182", SetName="Paradox Rift", Rarity="Illustration Rare", MarketPriceQar=110m, ImageUrl="https://images.pokemontcg.io/sv4/199.png" },
            new Card { Name="Walking Wake ex", CardNumber="205/162", SetName="Temporal Forces", Rarity="Special Illustration Rare", MarketPriceQar=90m, ImageUrl="https://images.pokemontcg.io/sv5/205.png" },
            new Card { Name="Raging Bolt ex", CardNumber="208/162", SetName="Temporal Forces", Rarity="Special Illustration Rare", MarketPriceQar=125m, ImageUrl="https://images.pokemontcg.io/sv5/208.png" },
            new Card { Name="Greninja ex", CardNumber="214/167", SetName="Twilight Masquerade", Rarity="Special Illustration Rare", MarketPriceQar=390m, ImageUrl="https://images.pokemontcg.io/sv6/214.png" },
            new Card { Name="Carmine", CardNumber="217/167", SetName="Twilight Masquerade", Rarity="Special Illustration Rare", MarketPriceQar=155m, ImageUrl="https://images.pokemontcg.io/sv6/217.png" },
            new Card { Name="Pikachu ex", CardNumber="238/191", SetName="Surging Sparks", Rarity="Special Illustration Rare", MarketPriceQar=520m, ImageUrl="https://images.pokemontcg.io/sv8/238.png" },
            new Card { Name="Latias ex", CardNumber="239/191", SetName="Surging Sparks", Rarity="Special Illustration Rare", MarketPriceQar=280m, ImageUrl="https://images.pokemontcg.io/sv8/239.png" },
            new Card { Name="Hydreigon ex", CardNumber="240/191", SetName="Surging Sparks", Rarity="Special Illustration Rare", MarketPriceQar=135m, ImageUrl="https://images.pokemontcg.io/sv8/240.png" },
            new Card { Name="Scarlet & Violet Booster Pack", CardNumber="PACK-SV1", SetName="Scarlet & Violet", Rarity="Sealed Product", ProductType="Booster Pack", MarketPriceQar=22m, ImageUrl="https://placehold.co/360x480?text=SV+Booster+Pack" },
            new Card { Name="151 Booster Pack", CardNumber="PACK-151", SetName="151", Rarity="Sealed Product", ProductType="Booster Pack", MarketPriceQar=38m, ImageUrl="https://placehold.co/360x480?text=151+Booster+Pack" },
            new Card { Name="Obsidian Flames Booster Pack", CardNumber="PACK-OF", SetName="Obsidian Flames", Rarity="Sealed Product", ProductType="Booster Pack", MarketPriceQar=24m, ImageUrl="https://placehold.co/360x480?text=Obsidian+Flames" },
            new Card { Name="151 Elite Trainer Box", CardNumber="ETB-151", SetName="151", Rarity="Sealed Product", ProductType="Trainer Box", MarketPriceQar=390m, ImageUrl="https://placehold.co/360x480?text=151+Elite+Trainer+Box" },
            new Card { Name="Surging Sparks Elite Trainer Box", CardNumber="ETB-SV8", SetName="Surging Sparks", Rarity="Sealed Product", ProductType="Trainer Box", MarketPriceQar=310m, ImageUrl="https://placehold.co/360x480?text=Surging+Sparks+ETB" }
        };

        var existing = db.Cards.Select(c => new { c.CardNumber, c.SetName }).ToList();
        foreach (var card in demoCards)
        {
            if (!existing.Any(x => x.CardNumber == card.CardNumber && x.SetName == card.SetName))
                db.Cards.Add(card);
        }

        db.SaveChanges();
    }
}