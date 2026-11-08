namespace PokeMarketApi.Models;
public class ConversationMessage
{
    public int Id { get; set; }
    public string SenderId { get; set; } = "";
    public string RecipientId { get; set; } = "";
    public string Message { get; set; } = "";
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}