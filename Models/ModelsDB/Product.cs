namespace clothes_bot.Models.ModelsDB
{
    public partial class Product
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string Description { get; set; } = null!;
        public string? Image { get; set; } = null!;
    }
}
