using System.Collections.Generic;

namespace PUTP2.Models
{
    public class UploadViewModel
    {
        public List<Models.PlaylistInfo> Playlists { get; set; }
        public string SelectedPlaylistId { get; set; }
        public bool IsPlaylistSelected { get; set; }
    }
} 