using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PUTP2.Data;

namespace PUTP2.Pages
{
    public class MusicModel : PageModel
    {
        public string PlaylistTitle { get; set; }
        public int SongCount { get; set; }
        public string TotalDuration { get; set; }
        public List<SongInfo> Songs { get; set; }

        public IActionResult OnGet(string playlistId)
        {
            if (!StaticData.Playlists.ContainsKey(playlistId))
            {
                return NotFound();
            }

            var playlist = StaticData.Playlists[playlistId];
            PlaylistTitle = playlist.Title;
            SongCount = playlist.SongCount;
            TotalDuration = playlist.TotalDuration;
            Songs = playlist.Songs;

            return Page();
        }
    }
} 