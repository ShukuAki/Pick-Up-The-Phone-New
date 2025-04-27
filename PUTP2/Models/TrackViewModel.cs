using System;

namespace PUTP2.Models
{
    public class TrackViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string UserTag { get; set; }
        public string Location { get; set; }
        public string ImageUrl { get; set; }
        public string AudioUrl { get; set; }
        public DateTime UploadDate { get; set; }
        public string Duration { get; set; }
        public string PlaylistTitle { get; set; }
    }
} 