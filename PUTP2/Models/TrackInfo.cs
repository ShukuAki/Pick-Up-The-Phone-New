namespace PUTP2.Models
{
    public class TrackInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string FilePath { get; set; }
        public DateTime UploadDate { get; set; }
        public string Duration { get; set; }
        public bool IsRecording { get; set; }
        public string Artist { get; set; }
    }
} 