using System.Collections.Generic;

namespace PUTP2.Models
{
    public class TuneInViewModel
    {
        public IEnumerable<Track> Tracks { get; set; }
        public IEnumerable<PlaylistInfo> Playlists { get; set; }
    }
} 