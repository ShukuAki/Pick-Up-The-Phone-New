using System.Collections.Generic;

namespace PUTP2.Models
{
    public class TuneInViewModel
    {
        public IEnumerable<PlaylistInfo> Playlists { get; set; }
        public IEnumerable<TrackInfo> Tracks { get; set; }
    }
} 