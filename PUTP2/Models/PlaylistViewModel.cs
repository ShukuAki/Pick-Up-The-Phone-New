using System.Collections.Generic;

namespace PUTP2.Models
{
    public class PlaylistViewModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string CoverUrl { get; set; }
        public int TrackCount { get; set; }
        public string Duration { get; set; }
        public List<Track> Tracks { get; set; } = new List<Track>();
    }
} 