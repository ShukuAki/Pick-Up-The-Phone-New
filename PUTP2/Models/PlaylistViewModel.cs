using System.Collections.Generic;

namespace PUTP2.Models
{
    public class PlaylistViewModel
    {
        public PlaylistInfo Playlist { get; set; }
        public List<TrackInfo> Tracks { get; set; } = new List<TrackInfo>();
    }
} 