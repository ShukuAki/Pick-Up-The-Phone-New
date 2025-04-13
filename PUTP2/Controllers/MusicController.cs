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
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PUTP2.Controllers
{//tocommit
    public class MusicController : Controller
    {
        private readonly PlaylistStorageService _storageService;
        private List<Track> _tracks;
        private readonly string playlistsPath = Path.Combine("Data", "playlists.json");
        private readonly string tracksJsonPath = Path.Combine("Data", "tracks.json");
        private readonly string uploadsPath = Path.Combine("Data", "UploadedSongs");
        private IEnumerable<Models.PlaylistInfo> _playlists;
        private int _nextPlaylistId;

        public MusicController(PlaylistStorageService storageService)
        {
            _storageService = storageService;
            _playlists = LoadData();
        }


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
            try
            {
                var playlistsJson = System.IO.File.ReadAllText(playlistsPath);
                var playlists = JsonSerializer.Deserialize<List<PUTP2.Models.PlaylistInfo>>(playlistsJson)
                    ?? new List<PUTP2.Models.PlaylistInfo>();
                return View(playlists);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading playlists: {ex.Message}");
                return View(new List<PUTP2.Models.PlaylistInfo>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveRecording([FromForm] IFormFile audioFile, string trackName, string playlistId)
        {
            try
            {
                if (string.IsNullOrEmpty(playlistId))
                    return Json(new { success = false, message = "Playlist ID is required" });

                if (audioFile == null || audioFile.Length == 0)
                    return Json(new { success = false, message = "No recording data received" });

                Directory.CreateDirectory(uploadsPath);

                var fileName = $"recording_{Guid.NewGuid()}.mp3";
                var filePath = Path.Combine(uploadsPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await audioFile.CopyToAsync(stream);
                }

                // Read existing tracks
                List<PUTP2.Models.TrackInfo> tracks;
                if (System.IO.File.Exists(tracksJsonPath))
                {
                    var json = await System.IO.File.ReadAllTextAsync(tracksJsonPath);
                    tracks = JsonSerializer.Deserialize<List<PUTP2.Models.TrackInfo>>(json) 
                        ?? new List<PUTP2.Models.TrackInfo>();
                }
                else
                {
                    tracks = new List<PUTP2.Models.TrackInfo>();
                }

                // Create new track
                var trackId = Guid.NewGuid().ToString();
                var newTrack = new PUTP2.Models.TrackInfo
                {
                    Id = trackId,
                    Name = trackName,
                    FilePath = filePath,
                    UploadDate = DateTime.Now,
                    Duration = "0:00", // You might want to calculate actual duration
                    IsRecording = true
                };

                // Add new track to tracks list
                tracks.Add(newTrack);

                // Save updated tracks
                await System.IO.File.WriteAllTextAsync(
                    tracksJsonPath,
                    JsonSerializer.Serialize(tracks, new JsonSerializerOptions { WriteIndented = true })
                );

                // Update playlist
                var playlists = await System.IO.File.ReadAllTextAsync(playlistsPath);
                var playlistsList = JsonSerializer.Deserialize<List<PUTP2.Models.PlaylistInfo>>(playlists) 
                    ?? new List<PUTP2.Models.PlaylistInfo>();

                var playlist = playlistsList.FirstOrDefault(p => p.Id == playlistId);
                if (playlist != null)
                {
                    playlist.Tracks ??= new List<string>();
                    playlist.Tracks.Add(trackId);
                    playlist.TrackCount = playlist.Tracks.Count;

                    await System.IO.File.WriteAllTextAsync(
                        playlistsPath,
                        JsonSerializer.Serialize(playlistsList, new JsonSerializerOptions { WriteIndented = true })
                    );
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private List<PlaylistInfo> LoadPlaylists()
        {
            if (System.IO.File.Exists(playlistsPath))
            {
                var json = System.IO.File.ReadAllText(playlistsPath);
                return JsonSerializer.Deserialize<List<PlaylistInfo>>(json) ?? new List<PlaylistInfo>();
            }
            return new List<PlaylistInfo>();
        }

        private async Task SavePlaylists(List<PlaylistInfo> playlists)
        {
            await System.IO.File.WriteAllTextAsync(playlistsPath, 
                JsonSerializer.Serialize(playlists, new JsonSerializerOptions { WriteIndented = true }));
        }

        private async Task<List<PUTP2.Models.TrackInfo>> LoadTracks()
        {
            if (System.IO.File.Exists(tracksJsonPath))
            {
                var json = await System.IO.File.ReadAllTextAsync(tracksJsonPath);
                return JsonSerializer.Deserialize<List<PUTP2.Models.TrackInfo>>(json) 
                    ?? new List<PUTP2.Models.TrackInfo>();
            }
            return new List<PUTP2.Models.TrackInfo>();
        }

        private async Task SaveTracks(List<PUTP2.Models.TrackInfo> tracks)
        {
            await System.IO.File.WriteAllTextAsync(
                tracksJsonPath,
                JsonSerializer.Serialize(tracks, new JsonSerializerOptions { WriteIndented = true })
            );
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
        [ValidateAntiForgeryToken]
        [RequestFormLimits(MultipartBodyLengthLimit = 52428800)]
        [RequestSizeLimit(52428800)]
        public async Task<IActionResult> UploadTrack([FromForm] IFormFile trackFile, [FromForm] string trackName, [FromForm] string playlistId, [FromForm] bool isRecording = false)
        {
            try
            {
                if (string.IsNullOrEmpty(playlistId))
                {
                    return Json(new { success = false, message = "Playlist ID is required" });
                }

                if (trackFile == null || trackFile.Length == 0)
                {
                    return Json(new { success = false, message = "No file uploaded" });
                }

                // Define paths
                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "UploadedSongs");
                var tracksJsonPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "tracks.json");
                var playlistsJsonPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "playlists.json");

                // Ensure directories exist
                Directory.CreateDirectory(uploadsPath);

                // Generate unique filename
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(trackFile.FileName)}";
                var filePath = Path.Combine(uploadsPath, fileName);

                // Save the file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await trackFile.CopyToAsync(stream);
                }

                // Create track info
                var trackId = Guid.NewGuid().ToString();
                var newTrack = new TrackInfo
                {
                    Id = trackId,
                    Name = trackName,
                    FilePath = $"/Data/UploadedSongs/{fileName}",
                    UploadDate = DateTime.Now,
                    //Duration = "0:00",
                    IsRecording = isRecording
                };

                // Update tracks.json
                var tracks = new List<TrackInfo>();
                if (System.IO.File.Exists(tracksJsonPath))
                {
                    var tracksJson = await System.IO.File.ReadAllTextAsync(tracksJsonPath);
                    if (!string.IsNullOrEmpty(tracksJson))
                    {
                        tracks = JsonSerializer.Deserialize<List<TrackInfo>>(tracksJson) ?? new List<TrackInfo>();
                    }
                }
                tracks.Add(newTrack);
                await System.IO.File.WriteAllTextAsync(tracksJsonPath, 
                    JsonSerializer.Serialize(tracks, new JsonSerializerOptions { WriteIndented = true }));

                // Update playlists.json
                var playlists = new List<PlaylistInfo>();
                if (System.IO.File.Exists(playlistsJsonPath))
                {
                    var playlistsJson = await System.IO.File.ReadAllTextAsync(playlistsJsonPath);
                    if (!string.IsNullOrEmpty(playlistsJson))
                    {
                        playlists = JsonSerializer.Deserialize<List<PlaylistInfo>>(playlistsJson) ?? new List<PlaylistInfo>();
                    }
                }

                var playlist = playlists.FirstOrDefault(p => p.Id == playlistId);
                if (playlist != null)
                {
                    playlist.Tracks ??= new List<string>();
                    playlist.Tracks.Add(trackId);
                    playlist.TrackCount = playlist.Tracks.Count;
                    await System.IO.File.WriteAllTextAsync(playlistsJsonPath, 
                        JsonSerializer.Serialize(playlists, new JsonSerializerOptions { WriteIndented = true }));
                }

                return Json(new { success = true, message = "Track uploaded successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
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

        private IEnumerable<Models.PlaylistInfo> LoadData()
        {
            try
            {
                if (System.IO.File.Exists(playlistsPath))
                {
                    var json = System.IO.File.ReadAllText(playlistsPath);
                    return JsonSerializer.Deserialize<List<Models.PlaylistInfo>>(json) 
                        ?? new List<Models.PlaylistInfo>();
                }
                
                // If file doesn't exist, create directory and return empty list
                Directory.CreateDirectory(Path.GetDirectoryName(playlistsPath));
                return new List<Models.PlaylistInfo>();
            }
            catch (Exception ex)
            {
                // Log the error if you have logging configured
                Console.WriteLine($"Error loading playlists: {ex.Message}");
                return new List<Models.PlaylistInfo>();
            }
        }

        private void SaveData()
        {
            try
            {
                var json = JsonSerializer.Serialize(_playlists, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                System.IO.File.WriteAllText(playlistsPath, json);
            }
            catch (Exception ex)
            {
                // Log the error if you have logging configured
                Console.WriteLine($"Error saving playlists: {ex.Message}");
                throw; // Re-throw the exception to be handled by the calling method
            }
        }

        [HttpPost]
        public IActionResult CreatePlaylist([FromForm] string title, IFormFile coverFile = null)
        {
            try
            {
                string coverUrl = "/images/default-playlist.jpg";
                
                if (coverFile != null && coverFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "playlists");
                        Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = $"{Guid.NewGuid()}_{coverFile.FileName}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        coverFile.CopyTo(stream);
                    }

                    coverUrl = $"/uploads/playlists/{uniqueFileName}";
                }

                var playlists = _playlists.ToList();
                string newId = Guid.NewGuid().ToString();

                var newPlaylist = new Models.PlaylistInfo
                {
                    Id = newId,
                    Title = title,
                    CoverUrl = coverUrl,
                    Tracks = new List<string>(),
                    CreatedDate = DateTime.Now,
                    TrackCount = 0,
                    Duration = "0:00"
                };

                playlists.Add(newPlaylist);
                _playlists = playlists;
                
                SaveData(); // Save the updated playlists to JSON

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
                string playlistsPath = Path.Combine("Data", "playlists.json");
                string tracksPath = Path.Combine("Data", "tracks.json");

                // Read playlists from JSON
                List<Models.PlaylistInfo> playlists;
                if (System.IO.File.Exists(playlistsPath))
                {
                    var playlistsJson = System.IO.File.ReadAllText(playlistsPath);
                    playlists = JsonSerializer.Deserialize<List<Models.PlaylistInfo>>(playlistsJson) ?? new List<Models.PlaylistInfo>();
                }
                else
                {
                    return Json(new { success = false, message = "Playlists file not found" });
                }

                // Find the playlist to delete
                var playlist = playlists.FirstOrDefault(p => p.Id == id);
                if (playlist == null)
                {
                    return Json(new { success = false, message = "Playlist not found" });
                }

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
                playlists.Remove(playlist);

                // Save updated playlists back to JSON
                var playlistsJsonOutput = JsonSerializer.Serialize(playlists, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                System.IO.File.WriteAllText(playlistsPath, playlistsJsonOutput);

                // Handle associated tracks
                if (System.IO.File.Exists(tracksPath))
                {
                    var tracksJson = System.IO.File.ReadAllText(tracksPath);
                    var tracks = JsonSerializer.Deserialize<List<TrackInfo>>(tracksJson) ?? new List<TrackInfo>();

                    // Get tracks associated with this playlist
                    var playlistTrackIds = playlist.Tracks ?? new List<string>();
                    var tracksToDelete = tracks.Where(t => playlistTrackIds.Contains(t.Id)).ToList();

                    // Delete track files
                    foreach (var track in tracksToDelete)
                    {
                        if (!string.IsNullOrEmpty(track.FilePath) && System.IO.File.Exists(track.FilePath))
                        {
                            System.IO.File.Delete(track.FilePath);
                        }
                        tracks.Remove(track);
                    }

                    // Save updated tracks back to JSON
                    var tracksJsonOutput = JsonSerializer.Serialize(tracks, new JsonSerializerOptions 
                    { 
                        WriteIndented = true 
                    });
                    System.IO.File.WriteAllText(tracksPath, tracksJsonOutput);
                }

                    return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public IActionResult Playlist(string id)
        {
            try
            {
                // Read playlists.json
                var playlistsJson = System.IO.File.ReadAllText(playlistsPath);
                var playlists = JsonSerializer.Deserialize<List<PUTP2.Models.PlaylistInfo>>(playlistsJson)
                    ?? new List<PUTP2.Models.PlaylistInfo>();

                // Find the specific playlist
                var playlist = playlists.FirstOrDefault(p => p.Id == id);
            if (playlist == null)
            {
                    return NotFound("Playlist not found");
                }

                // Read tracks.json
                var tracksJson = System.IO.File.ReadAllText(tracksJsonPath);
                var allTracks = JsonSerializer.Deserialize<List<PUTP2.Models.TrackInfo>>(tracksJson)
                    ?? new List<PUTP2.Models.TrackInfo>();

                // Get tracks for this playlist and prepare audio URLs
                var playlistTracks = new List<PUTP2.Models.TrackInfo>();
                if (playlist.Tracks != null)
                {
                    foreach (var trackId in playlist.Tracks)
                    {
                        var track = allTracks.FirstOrDefault(t => t.Id == trackId);
                        if (track != null)
                        {
                            // Create a copy of the track with the updated file path
                            var trackWithUrl = new PUTP2.Models.TrackInfo
                            {
                                Id = track.Id,
                                Name = track.Name,
                                UploadDate = track.UploadDate,
                                Duration = track.Duration ?? "0:00",
                                IsRecording = track.IsRecording,
                                // Convert the file path to a URL
                                FilePath = Url.Action("GetAudio", "Music", new { fileName = Path.GetFileName(track.FilePath) })
                            };
                            playlistTracks.Add(trackWithUrl);
                        }
                    }
                }
            
            var viewModel = new PlaylistViewModel
            {
                    Playlist = playlist,
                Tracks = playlistTracks
            };

            return View(viewModel);
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error loading playlist: {ex.Message}");
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpPost]
        public IActionResult DeleteTrack(string trackId, string playlistId)
        {
            try
            {
                var playlistsJson = System.IO.File.ReadAllText(playlistsPath);
                var playlists = JsonSerializer.Deserialize<List<PUTP2.Models.PlaylistInfo>>(playlistsJson)
                    ?? new List<PUTP2.Models.PlaylistInfo>();

                var playlist = playlists.FirstOrDefault(p => p.Id == playlistId);
                if (playlist?.Tracks != null)
                {
                    playlist.Tracks.Remove(trackId);
                    playlist.TrackCount = playlist.Tracks.Count;

                    System.IO.File.WriteAllText(playlistsPath, 
                        JsonSerializer.Serialize(playlists, new JsonSerializerOptions { WriteIndented = true }));

                    var tracksJson = System.IO.File.ReadAllText(tracksJsonPath);
                    var tracks = JsonSerializer.Deserialize<List<PUTP2.Models.TrackInfo>>(tracksJson)
                        ?? new List<PUTP2.Models.TrackInfo>();

                    var track = tracks.FirstOrDefault(t => t.Id == trackId);
                    if (track != null)
                    {
                        if (System.IO.File.Exists(track.FilePath))
                        {
                            System.IO.File.Delete(track.FilePath);
                        }
                        tracks.Remove(track);
                        System.IO.File.WriteAllText(tracksJsonPath, 
                            JsonSerializer.Serialize(tracks, new JsonSerializerOptions { WriteIndented = true }));
                    }
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
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

        [HttpGet]
        [Route("Music/GetAudio/{fileName}")]
        public IActionResult GetAudio(string fileName)
        {
            try
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "UploadedSongs", fileName);
                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound($"Audio file not found: {fileName}");
                }

                return PhysicalFile(filePath, "audio/mpeg");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error accessing audio file: {ex.Message}");
            }
        }
    }

    public class DeleteTrackModel
    {
        public string TrackId { get; set; }
        public string PlaylistId { get; set; }
    }
} 