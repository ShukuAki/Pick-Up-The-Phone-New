using System;
using System.Collections.Generic;

namespace PUTP2.Models
{
    public class PlaylistInfo
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string CoverUrl { get; set; }
        public List<string> Tracks { get; set; } = new List<string>();
        public int TrackCount { get; set; }
        public string Duration { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Level { get; set; } = 1; // Default to Level 1
    }
} 