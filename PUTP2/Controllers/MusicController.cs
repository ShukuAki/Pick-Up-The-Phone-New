using Microsoft.AspNetCore.Mvc;
using PUTP2.Models;
using System.Collections.Generic;
using PUTP2.Data;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.IO;
using System;
using System.Globalization;
using PUTP2.Services;

namespace PUTP2.Controllers
{
    public class MusicController : Controller
    {
        private readonly PlaylistStorageService _storageService;
        private List<Track> _tracks;
        private List<PlaylistInfo> _playlists;
        private int _nextPlaylistId;

        public MusicController(PlaylistStorageService storageService)
        {
            _storageService = storageService;
            LoadData();
        }

        private void LoadData()
        {
            _playlists = _storageService.LoadPlaylists();
            _tracks = _storageService.LoadTracks();
            _nextPlaylistId = _playlists.Any() ? int.Parse(_playlists.Max(p => p.Id)) + 1 : 1;
        }

        private void SaveData()
        {
            _storageService.SavePlaylists(_playlists);
            _storageService.SaveTracks(_tracks);
        }

        // Temporary in-memory data (replace with database later)
        private static List<Track> _tracksMemory = new List<Track>
        {
            new Track { Id = 1, Title = "Shape of You", Artist = "Ed Sheeran", Album = "÷", Duration = "3:53", ImageUrl = "/images/shape-of-you.jpg" },
            new Track { Id = 2, Title = "Blinding Lights", Artist = "The Weeknd", Album = "After Hours", Duration = "3:20", ImageUrl = "/images/blinding-lights.jpg" },
            new Track { Id = 3, Title = "Dance Monkey", Artist = "Tones and I", Album = "The Kids Are Coming", Duration = "3:29", ImageUrl = "/images/dance-monkey.jpg" }
        };

        // Add this field to store playlists in memory
        private static List<PlaylistInfo> _playlistsMemory = new List<PlaylistInfo>();

        // The Vault - Main page showing all tracks
        public IActionResult Index()
        {
            ViewData["Title"] = "The Vault";
            ViewBag.Playlists = _playlists;
            return View(_tracks);
        }

        // Upload page
        public IActionResult Upload()
        {
            var viewModel = new UploadViewModel
            {
                Playlists = _playlists.ToList(),
                IsPlaylistSelected = false
            };
            return View(viewModel);
        }

        [HttpPost]
        public IActionResult SelectPlaylist(string playlistId, bool scroll = false)
        {
            var viewModel = new UploadViewModel
            {
                Playlists = _playlists.ToList(),
                SelectedPlaylistId = playlistId,
                IsPlaylistSelected = true
            };
            return View("Upload", viewModel);
        }

        private IEnumerable<PlaylistInfo> GetSamplePlaylists()
        {
            if (!_playlistsMemory.Any())
            {
                _playlistsMemory = new List<PlaylistInfo>
                {
                    new PlaylistInfo { Id = "1", Title = "Daily Mix", CoverUrl = "/images/playlist1.jpg", TrackCount = 12, Duration = "45 min", Action = "DailyMix" },
                    new PlaylistInfo { Id = "2", Title = "Chill Vibes", CoverUrl = "/images/playlist2.jpg", TrackCount = 8, Duration = "32 min", Action = "ChillVibes" },
                    new PlaylistInfo { Id = "3", Title = "Workout Mix", CoverUrl = "/images/playlist3.jpg", TrackCount = 15, Duration = "58 min", Action = "WorkoutMix" },
                    new PlaylistInfo { Id = "4", Title = "Focus Mode", CoverUrl = "/images/playlist4.jpg", TrackCount = 10, Duration = "40 min", Action = "FocusMode" },
                    new PlaylistInfo { Id = "5", Title = "Evening Jazz", CoverUrl = "/images/playlist5.jpg", TrackCount = 6, Duration = "28 min", Action = "EveningJazz" },
                    new PlaylistInfo { Id = "6", Title = "Indie Picks", CoverUrl = "/images/playlist6.jpg", TrackCount = 14, Duration = "52 min", Action = "IndiePicks" }
                };
            }
            return _playlistsMemory;
        }

        // Tune In page for live streams and radio
        public IActionResult TuneIn()
        {
            var viewModel = new TuneInViewModel
            {
                Tracks = _tracks,
                Playlists = _playlists
            };
            return View(viewModel);
        }

        private IEnumerable<Track> GetSampleTracks()
        {
            return new List<Track>
            {
                new Track { Title = "Sample Track 1", Artist = "Artist 1", CoverUrl = "/images/cover1.jpg", Duration = "3:45", AudioUrl = "/audio/track1.mp3" },
                new Track { Title = "Sample Track 2", Artist = "Artist 2", CoverUrl = "/images/cover2.jpg", Duration = "4:20", AudioUrl = "/audio/track2.mp3" },
                new Track { Title = "Sample Track 3", Artist = "Artist 3", CoverUrl = "/images/cover3.jpg", Duration = "3:15", AudioUrl = "/audio/track3.mp3" },
                new Track { Title = "Sample Track 4", Artist = "Artist 4", CoverUrl = "/images/cover4.jpg", Duration = "4:50", AudioUrl = "/audio/track4.mp3" },
                new Track { Title = "Sample Track 5", Artist = "Artist 5", CoverUrl = "/images/cover5.jpg", Duration = "3:30", AudioUrl = "/audio/track5.mp3" },
                new Track { Title = "Sample Track 6", Artist = "Artist 6", CoverUrl = "/images/cover6.jpg", Duration = "4:10", AudioUrl = "/audio/track6.mp3" },
                new Track { Title = "Sample Track 7", Artist = "Artist 7", CoverUrl = "/images/cover7.jpg", Duration = "3:55", AudioUrl = "/audio/track7.mp3" },
                new Track { Title = "Sample Track 8", Artist = "Artist 8", CoverUrl = "/images/cover8.jpg", Duration = "4:25", AudioUrl = "/audio/track8.mp3" },
                new Track { Title = "Sample Track 9", Artist = "Artist 9", CoverUrl = "/images/cover9.jpg", Duration = "3:40", AudioUrl = "/audio/track9.mp3" },
                new Track { Title = "Sample Track 10", Artist = "Artist 10", CoverUrl = "/images/cover10.jpg", Duration = "4:15", AudioUrl = "/audio/track10.mp3" },
                new Track { Title = "Sample Track 11", Artist = "Artist 11", CoverUrl = "/images/cover11.jpg", Duration = "3:50", AudioUrl = "/audio/track11.mp3" },
                new Track { Title = "Sample Track 12", Artist = "Artist 12", CoverUrl = "/images/cover12.jpg", Duration = "4:30", AudioUrl = "/audio/track12.mp3" }
            };
        }

        // Profile page
        public IActionResult Profile()
        {
            ViewData["Title"] = "Profile";
            return View();
        }

        // Action for playing a specific track
        public IActionResult Play(int id)
        {
            var track = _tracks.Find(t => t.Id == id);
            if (track == null)
                return NotFound();

            return Json(new { 
                success = true, 
                track = track 
            });
        }

        // Action for handling file uploads
        [HttpPost]
        public IActionResult UploadTrack([FromForm] Track track)
        {
            track.Id = _tracks.Count + 1;
            _tracks.Add(track);
            SaveData();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult NowPlaying(int id)
        {
            var track = _tracks.Find(t => t.Id == id);
            return View(track);
        }

        public IActionResult Browse()
        {
            return View(_tracks);
        }

        [HttpGet]
        public IActionResult Search(string query)
        {
            if (string.IsNullOrEmpty(query))
                return View(_tracks);

            var results = _tracks.FindAll(t => 
                t.Title.Contains(query, System.StringComparison.OrdinalIgnoreCase) || 
                t.Artist.Contains(query, System.StringComparison.OrdinalIgnoreCase) ||
                t.Album.Contains(query, System.StringComparison.OrdinalIgnoreCase));
            
            return View(results);
        }

        public IActionResult PlayTrack(int id)
        {
            // In a real application, you would fetch the track details from your database
            // For now, we'll use dummy data
            ViewBag.TrackTitle = "Message to your (future) grandchildren";
            ViewBag.TrackArtist = "USER TAG [Grandparent]";
            ViewBag.TrackImage = "/images/default-track.jpg"; // You'll need to add a default image
            ViewBag.IsPlaying = true;

            return View();
        }

        public IActionResult Vault()
        {
            return RedirectToPage("/Vault");
        }

        public IActionResult DailyMix()
        {
            return View("~/Views/Playlist/DailyMix.cshtml");
        }

        public IActionResult ChillVibes()
        {
            return View("~/Views/Playlist/ChillVibes.cshtml");
        }

        public IActionResult WorkoutMix()
        {
            return View("~/Views/Playlist/WorkoutMix.cshtml");
        }

        public IActionResult FocusMode()
        {
            return View("~/Views/Playlist/FocusMode.cshtml");
        }

        public IActionResult EveningJazz()
        {
            return View("~/Views/Playlist/EveningJazz.cshtml");
        }

        public IActionResult IndiePicks()
        {
            return View("~/Views/Playlist/IndiePicks.cshtml");
        }

        [HttpPost]
        public IActionResult CreatePlaylist([FromForm] string title, IFormFile coverFile = null)
        {
            try
            {
                string coverUrl = "/images/default-playlist.jpg";
                
                if (coverFile != null && coverFile.Length > 0)
                {
                    // Create uploads directory if it doesn't exist
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "playlists");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Generate unique filename
                    var uniqueFileName = $"{Guid.NewGuid()}_{coverFile.FileName}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    
                    // Save the file
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        coverFile.CopyTo(stream);
                    }

                    coverUrl = $"/uploads/playlists/{uniqueFileName}";
                }

                var newPlaylist = new PlaylistInfo
                {
                    Id = _nextPlaylistId.ToString(),
                    Title = title,
                    CoverUrl = coverUrl,
                    TrackCount = 0,
                    Duration = "0:00",
                    Action = $"Playlist{_nextPlaylistId}"
                };

                _playlists.Add(newPlaylist);
                _nextPlaylistId++;
                SaveData();

                return Json(new { success = true, playlist = newPlaylist });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult DeletePlaylist(string id)
        {
            try
            {
                var playlist = _playlists.FirstOrDefault(p => p.Id == id);
                if (playlist != null)
                {
                    // Delete playlist cover image if it exists and is not the default
                    if (!string.IsNullOrEmpty(playlist.CoverUrl) && !playlist.CoverUrl.Contains("default-playlist.jpg"))
                    {
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", playlist.CoverUrl.TrimStart('/'));
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                    }

                    // Remove playlist from the list
                    _playlists.Remove(playlist);

                    // Remove associated tracks
                    _tracks.RemoveAll(t => t.PlaylistId == id);
                    SaveData();

                    return Json(new { success = true });
                }
                return Json(new { success = false, message = "Playlist not found" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public IActionResult Playlist(string id)
        {
            var playlist = _playlists.FirstOrDefault(p => p.Id == id);
            if (playlist == null)
            {
                return NotFound();
            }

            var playlistTracks = _tracks.Where(t => t.PlaylistId == id).ToList();
            
            var viewModel = new PlaylistViewModel
            {
                Id = playlist.Id,
                Title = playlist.Title,
                CoverUrl = playlist.CoverUrl,
                TrackCount = playlistTracks.Count,
                Duration = CalculateTotalDuration(playlistTracks),
                Tracks = playlistTracks
            };

            return View(viewModel);
        }

        private string CalculateTotalDuration(List<Track> tracks)
        {
            if (tracks == null || !tracks.Any())
                return "0:00";

            var totalSeconds = tracks.Sum(track =>
            {
                var parts = track.Duration.Split(':');
                if (parts.Length == 2 && int.TryParse(parts[0], out int minutes) && int.TryParse(parts[1], out int seconds))
                {
                    return minutes * 60 + seconds;
                }
                return 0;
            });

            var totalMinutes = totalSeconds / 60;
            var remainingSeconds = totalSeconds % 60;
            return $"{totalMinutes}:{remainingSeconds:00}";
        }
    }
} 