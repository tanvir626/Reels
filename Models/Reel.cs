namespace Reels.Models
{
    public class Reel
    {
        public int Id { get; set; }
        public string VideoId { get; set; }
        public string Title { get; set; }
        public string Tags { get; set; }
        public DateTime PublishedAt { get; set; }
        public string Source { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
