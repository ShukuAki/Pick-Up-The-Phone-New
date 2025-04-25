using System;

namespace PUTP2.Models
{
    public class TrackInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string FilePath { get; set; }
        public DateTime UploadDate { get; set; }
        public string Duration { get; set; } = "0:00";
        public bool IsRecording { get; set; }
        public string Artist { get; set; }
        public string UserTag { get; set; }
        public string Location { get; set; } = "Unknown";
        public string ImageUrl { get; set; } = "/images/default-album-cover.jpg";
        public string AgeGroup { get; set; }

        public TrackInfo()
        {
            UploadDate = DateTime.Now;
        }
    }
} 