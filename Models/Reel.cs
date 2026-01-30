using System.ComponentModel.DataAnnotations.Schema;

namespace Reels.Models
{
    public class Reel
    {
        public int Id { get; set; }
        public string VideoId { get; set; } = null!;
        public string Title { get; set; } = null!;
        public DateTime PublishedAt { get; set; }
        public string Source { get; set; } = "YouTube";

    }
}
