namespace PUTP2.Data
{
    public static class StaticData
    {
        public static Dictionary<string, PlaylistData> Playlists = new()
        {
            {
                "daily-mix",
                new PlaylistData
                {
                    Title = "daily mix",
                    SongCount = 12,
                    TotalDuration = "45 min",
                    CoverUrl = "/images/covers/daily-mix.jpg",
                    Songs = new List<SongInfo>
                    {
                        new() { Number = "1", Title = "Morning Coffee", Artist = "Chill Beats", Album = "Daily Rituals", Duration = "3:45", CoverUrl = "/images/covers/morning-coffee.jpg" },
                        new() { Number = "2", Title = "Urban Flow", Artist = "City Sounds", Album = "Metropolitan", Duration = "4:12", CoverUrl = "/images/covers/urban-flow.jpg" },
                        new() { Number = "3", Title = "Afternoon Groove", Artist = "Smooth Jazz", Album = "Jazz Hours", Duration = "3:55", CoverUrl = "/images/covers/afternoon-groove.jpg" }
                    }
                }
            },
            {
                "chill-vibes",
                new PlaylistData
                {
                    Title = "chill vibes",
                    SongCount = 8,
                    TotalDuration = "32 min",
                    CoverUrl = "/images/covers/chill-vibes.jpg",
                    Songs = new List<SongInfo>
                    {
                        new() { Number = "1", Title = "Ocean Waves", Artist = "Nature Sounds", Album = "Peaceful Moments", Duration = "4:15", CoverUrl = "/images/covers/ocean-waves.jpg" },
                        new() { Number = "2", Title = "Gentle Rain", Artist = "Ambient Noise", Album = "Natural Peace", Duration = "3:50", CoverUrl = "/images/covers/gentle-rain.jpg" },
                        new() { Number = "3", Title = "Sunset Melody", Artist = "Lofi Beats", Album = "Evening Calm", Duration = "4:05", CoverUrl = "/images/covers/sunset-melody.jpg" }
                    }
                }
            },
            {
                "workout-mix",
                new PlaylistData
                {
                    Title = "workout mix",
                    SongCount = 15,
                    TotalDuration = "58 min",
                    CoverUrl = "/images/covers/workout-mix.jpg",
                    Songs = new List<SongInfo>
                    {
                        new() { Number = "1", Title = "Power Up", Artist = "Energy Beats", Album = "Workout Energy", Duration = "3:30", CoverUrl = "/images/covers/power-up.jpg" },
                        new() { Number = "2", Title = "Running High", Artist = "Cardio Mix", Album = "Fitness Beats", Duration = "4:00", CoverUrl = "/images/covers/running-high.jpg" },
                        new() { Number = "3", Title = "Strength", Artist = "Gym Heroes", Album = "Iron Pumping", Duration = "3:45", CoverUrl = "/images/covers/strength.jpg" }
                    }
                }
            },
            {
                "focus-mode",
                new PlaylistData
                {
                    Title = "focus mode",
                    SongCount = 10,
                    TotalDuration = "40 min",
                    CoverUrl = "/images/covers/focus-mode.jpg",
                    Songs = new List<SongInfo>
                    {
                        new() { Number = "1", Title = "Deep Focus", Artist = "Study Beats", Album = "Concentration", Duration = "4:30", CoverUrl = "/images/covers/deep-focus.jpg" },
                        new() { Number = "2", Title = "Mind Clear", Artist = "Brain Waves", Album = "Mental Space", Duration = "4:15", CoverUrl = "/images/covers/mind-clear.jpg" },
                        new() { Number = "3", Title = "Flow State", Artist = "Alpha Waves", Album = "Pure Focus", Duration = "3:55", CoverUrl = "/images/covers/flow-state.jpg" }
                    }
                }
            },
            {
                "evening-jazz",
                new PlaylistData
                {
                    Title = "evening jazz",
                    SongCount = 6,
                    TotalDuration = "28 min",
                    CoverUrl = "/images/covers/evening-jazz.jpg",
                    Songs = new List<SongInfo>
                    {
                        new() { Number = "1", Title = "Night in Paris", Artist = "Jazz Quartet", Album = "Evening Standards", Duration = "4:45", CoverUrl = "/images/covers/night-paris.jpg" },
                        new() { Number = "2", Title = "Moonlight Sax", Artist = "Smooth Jazz", Album = "Late Night Jazz", Duration = "5:10", CoverUrl = "/images/covers/moonlight-sax.jpg" },
                        new() { Number = "3", Title = "Jazz Cafe", Artist = "The Jazz Band", Album = "Coffee & Jazz", Duration = "4:25", CoverUrl = "/images/covers/jazz-cafe.jpg" }
                    }
                }
            },
            {
                "indie-picks",
                new PlaylistData
                {
                    Title = "indie picks",
                    SongCount = 14,
                    TotalDuration = "52 min",
                    CoverUrl = "/images/covers/indie-picks.jpg",
                    Songs = new List<SongInfo>
                    {
                        new() { Number = "1", Title = "Indie Dreams", Artist = "The Locals", Album = "Underground", Duration = "3:45", CoverUrl = "/images/covers/indie-dreams.jpg" },
                        new() { Number = "2", Title = "Garage Band", Artist = "Street Artists", Album = "Raw Sound", Duration = "4:20", CoverUrl = "/images/covers/garage-band.jpg" },
                        new() { Number = "3", Title = "Alternative Ways", Artist = "New Wave", Album = "Different Path", Duration = "3:55", CoverUrl = "/images/covers/alternative-ways.jpg" }
                    }
                }
            }
        };
    }

    public class PlaylistData
    {
        public string Title { get; set; }
        public int SongCount { get; set; }
        public string TotalDuration { get; set; }
        public string CoverUrl { get; set; }
        public List<SongInfo> Songs { get; set; }
    }

    public class SongInfo
    {
        public string Number { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public string Duration { get; set; }
        public string CoverUrl { get; set; }
    }
} 