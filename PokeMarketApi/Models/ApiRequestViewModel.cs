using System.ComponentModel.DataAnnotations;

namespace PokeMarketApi.Models;

public class ApiRequestViewModel
{
    [Required, StringLength(80, MinimumLength = 2)]
    [Display(Name = "Application / project name")]
    public string ApplicationName { get; set; } = "";

    [Required, EmailAddress, StringLength(180)]
    [Display(Name = "Contact email")]
    public string ContactEmail { get; set; } = "";

    [Url, StringLength(250)]
    [Display(Name = "Website (optional)")]
    public string? Website { get; set; }

    [Required, StringLength(1000, MinimumLength = 20)]
    [Display(Name = "How will you use the PokéMarket API?")]
    public string Purpose { get; set; } = "";

    public string? ApiUrl { get; set; }
    public string? ApiKey { get; set; }
    public bool Submitted { get; set; }
}
