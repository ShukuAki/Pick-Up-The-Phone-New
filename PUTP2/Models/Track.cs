using System;

namespace PUTP2.Models
{
    public class Track
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public string Duration { get; set; }
        public string ImageUrl { get; set; }
        public string AudioUrl { get; set; }
        public string CoverUrl { get; set; }
        public string PlaylistId { get; set; }
    }
} 