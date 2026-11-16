namespace KhoyaPaya.Models
{
    public class ItemModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; } 
        public string ImagePath { get; set; }
        public string PostedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}