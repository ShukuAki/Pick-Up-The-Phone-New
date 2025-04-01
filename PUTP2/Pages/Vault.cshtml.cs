using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PUTP2.Pages
{
    public class VaultModel : PageModel
    {
        public List<VinylInfo> Vinyls { get; set; }

        public void OnGet()
        {
            // TODO: Fetch vinyl list from your database
            // This is a placeholder implementation
            Vinyls = new List<VinylInfo>
            {
                new VinylInfo 
                { 
                    Id = "1",
                    Title = "Sample Album 1",
                    Artist = "Artist 1",
                    CoverImageUrl = "/path/to/cover1.jpg"
                },
                new VinylInfo 
                { 
                    Id = "2",
                    Title = "Sample Album 2",
                    Artist = "Artist 2",
                    CoverImageUrl = "/path/to/cover2.jpg"
                }
            };
        }
    }

    public class VinylInfo
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string CoverImageUrl { get; set; }
    }
} 