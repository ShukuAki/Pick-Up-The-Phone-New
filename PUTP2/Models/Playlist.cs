using System.Collections.Generic;

namespace PUTP2.Models
{
    public class Playlist
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string CoverUrl { get; set; }
        public int TrackCount { get; set; }
        public string Duration { get; set; }
        public string Action { get; set; }
    }
} 