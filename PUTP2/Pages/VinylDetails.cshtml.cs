using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PUTP2.Models;
using System.Collections.Generic;

namespace PUTP2.Pages
{
    public class VinylDetailsModel : PageModel
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public string CoverUrl { get; set; }
        public string ReleaseDate { get; set; }
        public string Genre { get; set; }
        public string Duration { get; set; }
        public string Format { get; set; }
        public string Description { get; set; }
        public List<Track> Tracks { get; set; }

        public void OnGet(string id)
        {
            // TODO: Fetch vinyl details from your database using the id parameter
            // This is a placeholder implementation
            Title = "Sample Album";
            Artist = "Sample Artist";
            CoverUrl = "/images/vinyl1.jpg";
            ReleaseDate = "2024";
            Genre = "Rock";
            Duration = "45:30";
            Format = "Vinyl LP";
            Description = "This is a sample album description. Replace this with actual album details from your database.";
            
            Tracks = new List<Track>
            {
                new Track { Title = "Track 1", Artist = "Artist 1", Album = "Album 1", Duration = "3:45", CoverUrl = "/images/track1.jpg", AudioUrl = "/audio/track1.mp3" },
                new Track { Title = "Track 2", Artist = "Artist 2", Album = "Album 1", Duration = "4:20", CoverUrl = "/images/track2.jpg", AudioUrl = "/audio/track2.mp3" },
                new Track { Title = "Track 3", Artist = "Artist 3", Album = "Album 1", Duration = "3:15", CoverUrl = "/images/track3.jpg", AudioUrl = "/audio/track3.mp3" }
            };
        }
    }
} 