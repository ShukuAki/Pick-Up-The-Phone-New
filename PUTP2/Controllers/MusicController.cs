using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using PUTP2.Models;
using PUTP2.Services;
using PUTP2.Data;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Hosting;

namespace PUTP2.Controllers
{//tocommit
    public class MusicController : Controller
    {
        private readonly PlaylistStorageService _storageService;
        private readonly IWebHostEnvironment _env;
        private readonly string playlistsPath = Path.Combine("Data", "playlists.json");
        private readonly string tracksJsonPath = Path.Combine("Data", "tracks.json");
        private readonly string uploadsPath = Path.Combine("Data", "UploadedSongs");
        private readonly string tuneInUploadsPath = Path.Combine("Data", "TuneInSongs");
        private readonly string tuneInTracksJsonPath = Path.Combine("Data", "tuneInTracks.json");
        private List<PlaylistInfo> _playlists;
        // Static set to store IDs of visible playlists (Vault cards)
        private static HashSet<string> _visiblePlaylistIds = new HashSet<string>();

        public MusicController(PlaylistStorageService storageService, IWebHostEnvironment env)
        {
            _storageService = storageService;
            _env = env;
            _playlists = LoadData().ToList();

            // Ensure required directories exist
            Directory.CreateDirectory(Path.GetDirectoryName(playlistsPath));
            Directory.CreateDirectory(Path.GetDirectoryName(tracksJsonPath));
            Directory.CreateDirectory(Path.GetDirectoryName(tuneInTracksJsonPath));
            Directory.CreateDirectory(uploadsPath);
            Directory.CreateDirectory(tuneInUploadsPath);
        }


        // Add this field to store playlists in memory
        private static List<PlaylistInfo> _playlistsMemory = new List<PlaylistInfo>();

        // The Vault - Main page showing all tracks
        public IActionResult Index()
        {
            ViewData["Title"] = "The Vault";
            var allPlaylists = LoadAllPlaylists();
            // Filter playlists based on the static visible set
            var visiblePlaylists = allPlaylists.Where(p => _visiblePlaylistIds.Contains(p.Id)).ToList();
            ViewBag.Playlists = visiblePlaylists; // Pass only visible playlists to the view
            return View();
        }

        // Upload page
        [HttpGet]
        public IActionResult Upload()
        {
            ViewData["Title"] = "Add Card";
            var allPlaylists = LoadAllPlaylists();
            // Filter playlists to show only those NOT currently visible
            var hiddenPlaylists = allPlaylists.Where(p => !_visiblePlaylistIds.Contains(p.Id)).ToList();
            // Pass the hidden playlists to the view for the dropdown
            return View(hiddenPlaylists); // Pass the list as the model
        }

        // Tune In Upload page
        public IActionResult TuneInUpload()
        {
            var allPlaylists = LoadAllPlaylists();
            var viewModel = new UploadViewModel(_env)
            {
                Playlists = allPlaylists, // Keep loading all for now
                IsPlaylistSelected = false
            };
            return View(viewModel);
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
                List<TrackInfo> tracks;
                if (System.IO.File.Exists(tracksJsonPath))
                {
                    var json = await System.IO.File.ReadAllTextAsync(tracksJsonPath);
                    tracks = JsonSerializer.Deserialize<List<TrackInfo>>(json) 
                        ?? new List<TrackInfo>();
                }
                else
                {
                    tracks = new List<TrackInfo>();
                }

                // Create new track
                var trackId = Guid.NewGuid().ToString();
                var newTrack = new TrackInfo
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
                var playlistsList = JsonSerializer.Deserialize<List<PlaylistInfo>>(playlists) 
                    ?? new List<PlaylistInfo>();

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

        private async Task<List<TrackInfo>> LoadTracks()
        {
            if (System.IO.File.Exists(tracksJsonPath))
            {
                var json = await System.IO.File.ReadAllTextAsync(tracksJsonPath);
                return JsonSerializer.Deserialize<List<TrackInfo>>(json) 
                    ?? new List<TrackInfo>();
            }
            return new List<TrackInfo>();
        }

        private async Task SaveTracks(List<TrackInfo> tracks)
        {
            await System.IO.File.WriteAllTextAsync(
                tracksJsonPath,
                JsonSerializer.Serialize(tracks, new JsonSerializerOptions { WriteIndented = true })
            );
        }

        [HttpPost]
        public IActionResult SelectPlaylist(string playlistId, bool scroll = false)
        {
            var viewModel = new UploadViewModel(_env)
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
            ViewData["Title"] = "tune in";
            var allPlaylists = LoadAllPlaylists();
            var viewModel = new TuneInViewModel
            {
                Playlists = allPlaylists // Or apply different logic if TuneIn uses playlists differently
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
        public IActionResult Play(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest("Track ID is required");
                }

                // Load tracks from JSON file
                var tracks = System.IO.File.Exists(tracksJsonPath)
                    ? JsonSerializer.Deserialize<List<TrackInfo>>(System.IO.File.ReadAllText(tracksJsonPath))
                    : new List<TrackInfo>();

                var track = tracks?.FirstOrDefault(t => t.Id == id);
                if (track == null)
                {
                    return NotFound($"Track with ID {id} not found");
                }

                // Redirect to the PlayTrack view
                return RedirectToAction("PlayTrack", new { id = track.Id });
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error in Play: {ex.Message}");
                return RedirectToAction("Error", "Home", new { message = "Error loading track" });
            }
        }

        // Action for handling file uploads
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestFormLimits(MultipartBodyLengthLimit = 52428800)]
        [RequestSizeLimit(52428800)]
        public async Task<IActionResult> UploadTrack([FromForm] IFormFile trackFile, [FromForm] string trackName, [FromForm] string playlistId, [FromForm] bool isRecording = false, [FromForm] bool isTuneIn = false, [FromForm] string duration = "0:00")
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

                // Define paths based on whether it's a Tune In track
                var targetUploadsPath = isTuneIn ? tuneInUploadsPath : uploadsPath;
                var targetTracksJsonPath = isTuneIn ? tuneInTracksJsonPath : tracksJsonPath;

                // Ensure directories exist
                Directory.CreateDirectory(targetUploadsPath);

                // Generate unique filename
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(trackFile.FileName)}";
                var filePath = Path.Combine(targetUploadsPath, fileName);

                try
                {
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
                        FilePath = filePath,
                        UploadDate = DateTime.Now,
                        IsRecording = isRecording,
                        Duration = duration
                    };

                    // Update tracks.json
                    var tracks = new List<TrackInfo>();
                    if (System.IO.File.Exists(targetTracksJsonPath))
                    {
                        var tracksJson = await System.IO.File.ReadAllTextAsync(targetTracksJsonPath);
                        tracks = JsonSerializer.Deserialize<List<TrackInfo>>(tracksJson) ?? new List<TrackInfo>();
                    }
                    tracks.Add(newTrack);
                    await System.IO.File.WriteAllTextAsync(targetTracksJsonPath, 
                        JsonSerializer.Serialize(tracks, new JsonSerializerOptions { WriteIndented = true }));

                    // Update playlists.json
                    var playlists = new List<PlaylistInfo>();
                    if (System.IO.File.Exists(playlistsPath))
                    {
                        var playlistsJson = await System.IO.File.ReadAllTextAsync(playlistsPath);
                        playlists = JsonSerializer.Deserialize<List<PlaylistInfo>>(playlistsJson) ?? new List<PlaylistInfo>();
                    }

                    var playlist = playlists.FirstOrDefault(p => p.Id == playlistId);
                    if (playlist != null)
                    {
                        playlist.Tracks ??= new List<string>();
                        playlist.Tracks.Add(trackId);
                        playlist.TrackCount = playlist.Tracks.Count;
                        await System.IO.File.WriteAllTextAsync(playlistsPath, 
                            JsonSerializer.Serialize(playlists, new JsonSerializerOptions { WriteIndented = true }));

                        return Json(new { success = true, message = "Track uploaded successfully" });
                    }
                    else
                    {
                        // Clean up the uploaded file if playlist not found
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                        return Json(new { success = false, message = "Playlist not found" });
                    }
                }
                catch (Exception ex)
                {
                    // Clean up the uploaded file if there's an error
                    if (System.IO.File.Exists(filePath))
                    {
                        try
                        {
                            System.IO.File.Delete(filePath);
                        }
                        catch
                        {
                            // Log cleanup failure but don't throw
                            Console.WriteLine($"Failed to clean up file after error: {filePath}");
                        }
                    }
                    return Json(new { success = false, message = $"Error processing upload: {ex.Message}" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Upload failed: {ex.Message}" });
            }
        }

        public IActionResult NowPlaying(string id)
        {
            // Load tracks from JSON file
            var tracks = System.IO.File.Exists(tracksJsonPath)
                ? JsonSerializer.Deserialize<List<TrackInfo>>(System.IO.File.ReadAllText(tracksJsonPath))
                : new List<TrackInfo>();

            var track = tracks?.FirstOrDefault(t => t.Id == id);
            if (track == null)
            {
                return NotFound();
            }
            return View(track);
        }

        public IActionResult Browse()
        {
            var tracks = System.IO.File.Exists(tracksJsonPath)
                ? JsonSerializer.Deserialize<List<TrackInfo>>(System.IO.File.ReadAllText(tracksJsonPath))
                : new List<TrackInfo>();
            return View(tracks);
        }

        [HttpGet]
        public IActionResult Search(string query)
        {
            var tracks = System.IO.File.Exists(tracksJsonPath)
                ? JsonSerializer.Deserialize<List<TrackInfo>>(System.IO.File.ReadAllText(tracksJsonPath))
                : new List<TrackInfo>();

            if (string.IsNullOrEmpty(query))
                return View(tracks);

            var results = tracks.Where(t => 
                t.Name.Contains(query, StringComparison.OrdinalIgnoreCase) || 
                (t.Artist != null && t.Artist.Contains(query, StringComparison.OrdinalIgnoreCase))).ToList();
            
            return View(results);
        }

        public IActionResult PlayTrack(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest("Track ID is required");
                }

                if (!System.IO.File.Exists(tracksJsonPath))
                {
                    return NotFound("Tracks database not found");
                }

                var tracksJson = System.IO.File.ReadAllText(tracksJsonPath);
                if (string.IsNullOrEmpty(tracksJson))
                {
                    return NotFound("No tracks found");
                }

                var tracks = JsonSerializer.Deserialize<List<TrackInfo>>(tracksJson);
                if (tracks == null || !tracks.Any())
                {
                    return NotFound("No tracks available");
                }

                var track = tracks.FirstOrDefault(t => t.Id == id);
                if (track == null)
                {
                    return NotFound($"Track with ID {id} not found");
                }

                var audioFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "UploadedSongs", Path.GetFileName(track.FilePath ?? ""));
                if (string.IsNullOrEmpty(track.FilePath) || !System.IO.File.Exists(audioFilePath))
                {
                    return NotFound("Audio file not found");
                }

                var viewModel = new TrackViewModel
                {
                    Id = track.Id,
                    Name = track.Name ?? "Untitled Track",
                    UserTag = track.Artist ?? "USER TAG",
                    Location = "LOCATION",
                    ImageUrl = "/images/default-album-cover.jpg",
                    AudioUrl = $"/Music/GetAudio/{Path.GetFileName(track.FilePath)}",
                    UploadDate = track.UploadDate,
                    Duration = track.Duration ?? "0:00"
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in PlayTrack: {ex.Message}");
                return RedirectToAction("Error", "Home", new { message = "Error loading track" });
            }
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

        public IActionResult Level(int level)
        {
            if (level < 1 || level > 3)
            {
                return NotFound();
            }

            ViewBag.Level = level;
            ViewBag.Playlists = _playlists;
            return View();
        }

        private List<PlaylistInfo> LoadData()
        {
            try
            {
                if (System.IO.File.Exists(playlistsPath))
                {
                    var json = System.IO.File.ReadAllText(playlistsPath);
                    return JsonSerializer.Deserialize<List<PlaylistInfo>>(json) ?? new List<PlaylistInfo>();
                }
                
                // If file doesn't exist, create directory and return empty list
                Directory.CreateDirectory(Path.GetDirectoryName(playlistsPath));
                return new List<PlaylistInfo>();
            }
            catch (Exception ex)
            {
                // Log the error if you have logging configured
                Console.WriteLine($"Error loading playlists: {ex.Message}");
                return new List<PlaylistInfo>();
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

                var newPlaylist = new PlaylistInfo
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
        public async Task<IActionResult> DeletePlaylist(string id)
        {
            var playlists = LoadAllPlaylists();
            var playlistToRemove = playlists.FirstOrDefault(p => p.Id == id);

            if (playlistToRemove == null)
            {
                return Json(new { success = false, message = "Playlist not found." });
            }

            playlists.Remove(playlistToRemove);

            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(playlists, options);
                await System.IO.File.WriteAllTextAsync(playlistsPath, json);

                // Also remove from the visible set if it was there
                _visiblePlaylistIds.Remove(id);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving playlists: {ex.Message}");
                return Json(new { success = false, message = "Error saving playlist data." });
            }
        }

        public IActionResult Playlist(string id)
        {
            try
            {
                // Read playlists.json
                var playlistsJson = System.IO.File.ReadAllText(playlistsPath);
                var playlists = JsonSerializer.Deserialize<List<PlaylistInfo>>(playlistsJson)
                    ?? new List<PlaylistInfo>();

                // Find the specific playlist
                var playlist = playlists.FirstOrDefault(p => p.Id == id);
            if (playlist == null)
            {
                    return NotFound("Playlist not found");
                }

                // Read tracks.json
                var tracksJson = System.IO.File.ReadAllText(tracksJsonPath);
                var allTracks = JsonSerializer.Deserialize<List<TrackInfo>>(tracksJson)
                    ?? new List<TrackInfo>();

                // Get tracks for this playlist and prepare audio URLs
                var playlistTracks = new List<TrackInfo>();
                if (playlist.Tracks != null)
                {
                    foreach (var trackId in playlist.Tracks)
                    {
                        var track = allTracks.FirstOrDefault(t => t.Id == trackId);
                        if (track != null)
                        {
                            // Create a copy of the track with the updated file path
                            var trackWithUrl = new TrackInfo
                            {
                                Id = track.Id,
                                Name = track.Name,
                                UploadDate = track.UploadDate,
                                Duration = track.Duration ?? "0:00",
                                IsRecording = track.IsRecording,
                                // Convert the file path to a URL
                                FilePath = Url.Action("GetAudio", "Music", new { fileName = Path.GetFileName(track.FilePath), isTuneIn = false })
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
                var playlists = JsonSerializer.Deserialize<List<PlaylistInfo>>(playlistsJson)
                    ?? new List<PlaylistInfo>();

                var playlist = playlists.FirstOrDefault(p => p.Id == playlistId);
                if (playlist?.Tracks != null)
                {
                    playlist.Tracks.Remove(trackId);
                    playlist.TrackCount = playlist.Tracks.Count;

                    System.IO.File.WriteAllText(playlistsPath, 
                        JsonSerializer.Serialize(playlists, new JsonSerializerOptions { WriteIndented = true }));

                    var tracksJson = System.IO.File.ReadAllText(tracksJsonPath);
                    var tracks = JsonSerializer.Deserialize<List<TrackInfo>>(tracksJson)
                        ?? new List<TrackInfo>();

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

        private string CalculateTotalDuration(List<TrackInfo> tracks)
        {
            if (tracks == null || !tracks.Any())
                return "0:00";

            var totalSeconds = tracks.Sum(track =>
            {
                if (string.IsNullOrEmpty(track?.Duration)) return 0;
                try
                {
                    var parts = track.Duration.Split(':');
                    if (parts.Length == 2 && int.TryParse(parts[0], out int minutes) && int.TryParse(parts[1], out int seconds))
                    {
                        return minutes * 60 + seconds;
                    }
                }
                catch
                {
                    return 0;
                }
                return 0;
            });

            var totalMinutes = totalSeconds / 60;
            var remainingSeconds = totalSeconds % 60;
            return $"{totalMinutes}:{remainingSeconds:00}";
        }

        [HttpGet]
        [Route("Music/GetAudio/{fileName}")]
        public IActionResult GetAudio(string fileName, bool isTuneIn = false)
        {
            var basePath = isTuneIn ? Path.Combine(_env.ContentRootPath, "Data", "TuneInSongs") :
                                       Path.Combine(_env.ContentRootPath, "Data", "UploadedSongs");
            var filePath = Path.Combine(basePath, fileName);

            if (!System.IO.File.Exists(filePath))
            {
                 Console.WriteLine($"Audio file not found: {filePath}");
                return NotFound();
            }
            try
            {
                 return PhysicalFile(filePath, "audio/mpeg", enableRangeProcessing: true);
            }
             catch (Exception ex)
            {
                 Console.WriteLine($"Error serving audio file {filePath}: {ex.Message}");
                 return StatusCode(500, "Internal server error");
            }
        }

        public IActionResult TuneInPlaylist(string id)
        {
            try
            {
                Console.WriteLine($"TuneInPlaylist called with ID: {id}");
                
                if (string.IsNullOrEmpty(id))
                {
                    return RedirectToAction("TuneIn", new { error = "Invalid playlist ID" });
                }

                // Read playlists
                if (!System.IO.File.Exists(playlistsPath))
                {
                    Console.WriteLine($"Playlists file not found at: {playlistsPath}");
                    return RedirectToAction("TuneIn", new { error = "Playlists file not found" });
                }

                var playlistsJson = System.IO.File.ReadAllText(playlistsPath);
                var playlists = JsonSerializer.Deserialize<List<PlaylistInfo>>(playlistsJson) ?? new List<PlaylistInfo>();
                Console.WriteLine($"Found {playlists.Count} playlists");

                var playlist = playlists.FirstOrDefault(p => p.Id == id);
                if (playlist == null)
                {
                    Console.WriteLine($"Playlist with ID {id} not found");
                    return RedirectToAction("TuneIn", new { error = "Playlist not found" });
                }

                Console.WriteLine($"Found playlist: {playlist.Title} with {playlist.Tracks?.Count ?? 0} tracks");

                // Read tracks from tuneInTracksJsonPath
                if (!System.IO.File.Exists(tuneInTracksJsonPath))
                {
                    Console.WriteLine($"Tracks file not found at: {tuneInTracksJsonPath}");
                    return RedirectToAction("TuneIn", new { error = "Tracks file not found" });
                }

                var tracksJson = System.IO.File.ReadAllText(tuneInTracksJsonPath);
                var allTracks = JsonSerializer.Deserialize<List<TrackInfo>>(tracksJson) ?? new List<TrackInfo>();
                Console.WriteLine($"Found {allTracks.Count} total tracks");

                var playlistTracks = new List<TrackInfo>();
                if (playlist.Tracks?.Any() == true)
                {
                    foreach (var trackId in playlist.Tracks)
                    {
                        Console.WriteLine($"Processing track ID: {trackId}");
                        var track = allTracks.FirstOrDefault(t => t.Id == trackId);
                        if (track != null)
                        {
                            Console.WriteLine($"Found track: {track.Name} with FilePath: {track.FilePath}");
                            
                            // Extract the filename from the stored path, handling both forward and backslashes
                            var storedFileName = Path.GetFileName(track.FilePath.Replace('/', '\\'));
                            Console.WriteLine($"Extracted filename: {storedFileName}");

                            // Try to find the audio file by searching in the TuneInSongs directory
                            var audioFiles = Directory.GetFiles(tuneInUploadsPath, "*.mp3");
                            Console.WriteLine($"Found {audioFiles.Length} audio files in {tuneInUploadsPath}");
                            
                            // Try multiple matching strategies
                            string audioFileName = null;
                            
                            // Strategy 1: Exact match
                            var audioFile = audioFiles.FirstOrDefault(f => 
                                Path.GetFileName(f).Equals(storedFileName, StringComparison.OrdinalIgnoreCase));
                            
                            if (audioFile != null)
                            {
                                audioFileName = Path.GetFileName(audioFile);
                                Console.WriteLine($"Found exact match: {audioFileName}");
                            }
                            else
                            {
                                // Strategy 2: Match by GUID
                                var guidMatch = storedFileName.Split('_').Last().Replace(".mp3", "");
                                audioFile = audioFiles.FirstOrDefault(f => 
                                    Path.GetFileName(f).Contains(guidMatch, StringComparison.OrdinalIgnoreCase));
                                
                                if (audioFile != null)
                                {
                                    audioFileName = Path.GetFileName(audioFile);
                                    Console.WriteLine($"Found GUID match: {audioFileName}");
                                }
                                else
                                {
                                    // Strategy 3: Match by track ID
                                    audioFile = audioFiles.FirstOrDefault(f => 
                                        Path.GetFileName(f).Contains(trackId, StringComparison.OrdinalIgnoreCase));
                                    
                                    if (audioFile != null)
                                    {
                                        audioFileName = Path.GetFileName(audioFile);
                                        Console.WriteLine($"Found track ID match: {audioFileName}");
                                    }
                                }
                            }

                            if (!string.IsNullOrEmpty(audioFileName))
                            {
                                var audioFilePath = Path.Combine(tuneInUploadsPath, audioFileName);
                                
                                if (System.IO.File.Exists(audioFilePath))
                                {
                                    Console.WriteLine($"Audio file exists at: {audioFilePath}");
                                    
                                    // Create a copy of the track with the updated file path
                                    var trackWithUrl = new TrackInfo
                                    {
                                        Id = track.Id,
                                        Name = track.Name ?? "Untitled Track",
                                        UploadDate = track.UploadDate,
                                        Duration = !string.IsNullOrEmpty(track.Duration) ? track.Duration : "0:00",
                                        IsRecording = track.IsRecording,
                                        Artist = track.Artist ?? "Unknown Artist",
                                        UserTag = track.UserTag ?? track.Artist ?? "Unknown Artist",
                                        Location = track.Location ?? "Unknown Location",
                                        ImageUrl = !string.IsNullOrEmpty(track.ImageUrl) ? track.ImageUrl : "/images/default-album-cover.jpg",
                                        FilePath = Url.Action("GetAudio", "Music", new { fileName = audioFileName, isTuneIn = true })
                                    };
                                    Console.WriteLine($"Created track with URL: {trackWithUrl.FilePath}");
                                    playlistTracks.Add(trackWithUrl);
                                }
                                else
                                {
                                    Console.WriteLine($"Audio file does not exist at: {audioFilePath}");
                                }
                            }
                            else
                            {
                                Console.WriteLine($"No matching audio file found for {storedFileName}");
                                // Log all available files for debugging
                                Console.WriteLine("Available files:");
                                foreach (var file in audioFiles)
                                {
                                    Console.WriteLine($"- {Path.GetFileName(file)}");
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Track with ID {trackId} not found in tracks list");
                        }
                    }
                }

                var viewModel = new PlaylistViewModel
                {
                    Playlist = playlist,
                    Tracks = playlistTracks
                };

                Console.WriteLine($"Returning view with {playlistTracks.Count} tracks");
                return View("TuneInPlaylist", viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading Tune In playlist: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return RedirectToAction("TuneIn", new { error = "An error occurred while loading the playlist" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveTuneInRecording([FromForm] IFormFile audioFile, [FromForm] string trackName, [FromForm] string playlistId, [FromForm] string duration)
        {
            try
            {
                if (string.IsNullOrEmpty(playlistId))
                {
                    return Json(new { success = false, message = "Playlist ID is required" });
                }

                if (audioFile == null || audioFile.Length == 0)
                {
                    return Json(new { success = false, message = "No recording data received" });
                }

                // Ensure directory exists
                Directory.CreateDirectory(tuneInUploadsPath);

                // Generate a single GUID for both track ID and filename
                var trackId = Guid.NewGuid().ToString();
                var fileName = $"recording_{trackId}.mp3";
                var filePath = Path.Combine(tuneInUploadsPath, fileName);

                // Save the recording
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await audioFile.CopyToAsync(stream);
                }

                // Create track info
                var newTrack = new TrackInfo
                {
                    Id = trackId,
                    Name = trackName,
                    FilePath = fileName, // Store only the filename
                    UploadDate = DateTime.Now,
                    Duration = duration ?? "0:00",
                    IsRecording = true
                };

                // Update tracks.json
                var tracks = new List<TrackInfo>();
                if (System.IO.File.Exists(tuneInTracksJsonPath))
                {
                    var json = await System.IO.File.ReadAllTextAsync(tuneInTracksJsonPath);
                    tracks = JsonSerializer.Deserialize<List<TrackInfo>>(json) ?? new List<TrackInfo>();
                }
                tracks.Add(newTrack);
                await System.IO.File.WriteAllTextAsync(tuneInTracksJsonPath, 
                    JsonSerializer.Serialize(tracks, new JsonSerializerOptions { WriteIndented = true }));

                // Update playlist.json
                var playlists = new List<PlaylistInfo>();
                if (System.IO.File.Exists(playlistsPath))
                {
                    var json = await System.IO.File.ReadAllTextAsync(playlistsPath);
                    playlists = JsonSerializer.Deserialize<List<PlaylistInfo>>(json) ?? new List<PlaylistInfo>();
                }

                var playlist = playlists.FirstOrDefault(p => p.Id == playlistId);
                if (playlist != null)
                {
                    playlist.Tracks ??= new List<string>();
                    playlist.Tracks.Add(trackId);
                    playlist.TrackCount = playlist.Tracks.Count;
                    playlist.Duration = CalculateTotalDuration(tracks.Where(t => playlist.Tracks.Contains(t.Id)).ToList());
                    await System.IO.File.WriteAllTextAsync(playlistsPath, 
                        JsonSerializer.Serialize(playlists, new JsonSerializerOptions { WriteIndented = true }));
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public IActionResult PlayTuneInTrack(string id)
        {
            try
            {
                Console.WriteLine($"PlayTuneInTrack called with ID: {id}");
                
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest("Track ID is required");
                }

                // Read tracks from tuneInTracksJsonPath
                if (!System.IO.File.Exists(tuneInTracksJsonPath))
                {
                    Console.WriteLine($"Tracks file not found at: {tuneInTracksJsonPath}");
                    return NotFound("Tracks database not found");
                }

                var tracksJson = System.IO.File.ReadAllText(tuneInTracksJsonPath);
                var tracks = JsonSerializer.Deserialize<List<TrackInfo>>(tracksJson) ?? new List<TrackInfo>();

                var track = tracks.FirstOrDefault(t => t.Id == id);
                if (track == null)
                {
                    Console.WriteLine($"Track with ID {id} not found");
                    return NotFound($"Track with ID {id} not found");
                }

                Console.WriteLine($"Found track: {track.Name}");

                // Extract the filename from the stored path
                var storedFileName = Path.GetFileName(track.FilePath.Replace('/', '\\'));
                Console.WriteLine($"Looking for file: {storedFileName}");

                // Try to find the audio file
                var audioFiles = Directory.GetFiles(tuneInUploadsPath, "*.mp3");
                Console.WriteLine($"Found {audioFiles.Length} audio files in {tuneInUploadsPath}");

                string audioFileName = null;
                
                // Try multiple matching strategies
                var audioFile = audioFiles.FirstOrDefault(f => 
                    Path.GetFileName(f).Equals(storedFileName, StringComparison.OrdinalIgnoreCase));

                if (audioFile != null)
                {
                    audioFileName = Path.GetFileName(audioFile);
                    Console.WriteLine($"Found exact match: {audioFileName}");
                }
                else
                {
                    // Try matching by GUID
                    var guidMatch = storedFileName.Split('_').Last().Replace(".mp3", "");
                    audioFile = audioFiles.FirstOrDefault(f => 
                        Path.GetFileName(f).Contains(guidMatch, StringComparison.OrdinalIgnoreCase));

                    if (audioFile != null)
                    {
                        audioFileName = Path.GetFileName(audioFile);
                        Console.WriteLine($"Found GUID match: {audioFileName}");
                    }
                }

                if (string.IsNullOrEmpty(audioFileName))
                {
                    Console.WriteLine("No matching audio file found. Available files:");
                    foreach (var file in audioFiles)
                    {
                        Console.WriteLine($"- {Path.GetFileName(file)}");
                    }
                    return NotFound("Audio file not found");
                }

                var viewModel = new TrackViewModel
                {
                    Id = track.Id,
                    Name = track.Name ?? "Untitled Track",
                    UserTag = track.UserTag ?? track.Artist ?? "Unknown Artist",
                    Location = track.Location ?? "Unknown Location",
                    ImageUrl = !string.IsNullOrEmpty(track.ImageUrl) ? track.ImageUrl : "/images/default-album-cover.jpg",
                    AudioUrl = Url.Action("GetAudio", "Music", new { fileName = audioFileName, isTuneIn = true }),
                    UploadDate = track.UploadDate,
                    Duration = !string.IsNullOrEmpty(track.Duration) ? track.Duration : "0:00"
                };

                Console.WriteLine($"Returning view with audio URL: {viewModel.AudioUrl}");
                return View("PlayTuneInTrack", viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in PlayTuneInTrack: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return RedirectToAction("TuneIn", new { error = "Error loading track" });
            }
        }

        private List<PlaylistInfo> LoadAllPlaylists()
        {
            if (!System.IO.File.Exists(playlistsPath))
            {
                return new List<PlaylistInfo>();
            }
            try
            {
                var json = System.IO.File.ReadAllText(playlistsPath);
                return JsonSerializer.Deserialize<List<PlaylistInfo>>(json) ?? new List<PlaylistInfo>();
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error loading playlists from {playlistsPath}: {ex.Message}");
                return new List<PlaylistInfo>();
            }
        }

        // New POST Action to make a playlist visible
        [HttpPost]
        [ValidateAntiForgeryToken] // Good practice for POST actions
        public IActionResult MakeVisible(string playlistId)
        {
            if (!string.IsNullOrEmpty(playlistId))
            {
                // Add the selected ID to the visible set
                _visiblePlaylistIds.Add(playlistId);
            }
            // Redirect back to the Upload page to potentially add more
            return RedirectToAction("Upload");
        }
    }

    public class DeleteTrackModel
    {
        public string TrackId { get; set; }
        public string PlaylistId { get; set; }
    }
} 