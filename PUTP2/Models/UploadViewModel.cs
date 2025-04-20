using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

namespace PUTP2.Models
{
    public class UploadViewModel : PageModel
    {
        private readonly string _uploadsPath = Path.Combine("Data", "UploadedSongs");
        private readonly string _tracksJsonPath = Path.Combine("Data", "tracks.json");
        private readonly string _playlistsPath = Path.Combine("Data", "playlists.json");
        private readonly string _playlistCoversPath = Path.Combine("wwwroot", "images", "playlist-covers");
        private const string _playlistFileName = "wwwroot/data/playlists.json";
        private readonly IWebHostEnvironment _env;
        public List<PlaylistInfo> Playlists { get; set; } = new List<PlaylistInfo>();
        public string SelectedPlaylistId { get; set; }
        public bool IsPlaylistSelected { get; set; }

        public UploadViewModel(IWebHostEnvironment env)
        {
            _env = env;
        }

        public void OnGet()
        {
            try
            {
                var playlistsJson = System.IO.File.ReadAllText(_playlistsPath);
                Playlists = JsonSerializer.Deserialize<List<PlaylistInfo>>(playlistsJson) ?? new List<PlaylistInfo>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading playlists: {ex.Message}");
                Playlists = new List<PlaylistInfo>();
            }
        }

        public async Task<IActionResult> OnPostUploadTrackAsync()
        {
            try
            {
                var file = Request.Form.Files["trackFile"];
                var trackName = Request.Form["trackName"].ToString();
                var artist = Request.Form["artist"].ToString();

                if (file == null || file.Length == 0)
                    return BadRequest("No file uploaded");

                Directory.CreateDirectory(_uploadsPath);

                var fileName = $"{Guid.NewGuid()}.mp3";
                var filePath = Path.Combine(_uploadsPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var trackInfo = new TrackInfo
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = trackName,
                    Artist = artist,
                    FilePath = filePath,
                    UploadDate = DateTime.Now
                };

                var tracks = new List<TrackInfo>();
                if (System.IO.File.Exists(_tracksJsonPath))
                {
                    var json = await System.IO.File.ReadAllTextAsync(_tracksJsonPath);
                    tracks = JsonSerializer.Deserialize<List<TrackInfo>>(json) ?? new List<TrackInfo>();
                }

                tracks.Add(trackInfo);
                await System.IO.File.WriteAllTextAsync(_tracksJsonPath, 
                    JsonSerializer.Serialize(tracks, new JsonSerializerOptions { WriteIndented = true }));

                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        public async Task<JsonResult> OnPostSaveRecordingAsync()
        {
            try
            {
                var file = Request.Form.Files["audioFile"];
                var trackName = Request.Form["trackName"].ToString();
                var playlistId = Request.Form["playlistId"].ToString();

                if (string.IsNullOrEmpty(playlistId))
                    return new JsonResult(new { success = false, message = "Playlist ID is required" });

                if (file == null || file.Length == 0)
                    return new JsonResult(new { success = false, message = "No recording data received" });

                Directory.CreateDirectory(_uploadsPath);

                var fileName = $"recording_{Guid.NewGuid()}.mp3";
                var filePath = Path.Combine(_uploadsPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var trackId = Guid.NewGuid().ToString();
                var trackInfo = new TrackInfo
                {
                    Id = trackId,
                    Name = trackName,
                    FilePath = filePath,
                    UploadDate = DateTime.Now,
                    IsRecording = true
                };

                var tracks = new List<TrackInfo>();
                if (System.IO.File.Exists(_tracksJsonPath))
                {
                    var json = await System.IO.File.ReadAllTextAsync(_tracksJsonPath);
                    tracks = JsonSerializer.Deserialize<List<TrackInfo>>(json) ?? new List<TrackInfo>();
                }
                tracks.Add(trackInfo);
                await System.IO.File.WriteAllTextAsync(_tracksJsonPath, 
                    JsonSerializer.Serialize(tracks, new JsonSerializerOptions { WriteIndented = true }));

                var playlists = new List<PlaylistInfo>();
                if (System.IO.File.Exists(_playlistsPath))
                {
                    var json = await System.IO.File.ReadAllTextAsync(_playlistsPath);
                    playlists = JsonSerializer.Deserialize<List<PlaylistInfo>>(json) ?? new List<PlaylistInfo>();
                }

                var playlist = playlists.FirstOrDefault(p => p.Id == playlistId);
                if (playlist != null)
                {
                    playlist.Tracks ??= new List<string>();
                    playlist.Tracks.Add(trackId);
                    playlist.TrackCount = playlist.Tracks.Count;
                    await System.IO.File.WriteAllTextAsync(_playlistsPath, 
                        JsonSerializer.Serialize(playlists, new JsonSerializerOptions { WriteIndented = true }));
                }

                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        public IActionResult OnPostCreatePlaylist([FromBody] CreatePlaylistRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return new JsonResult(new { success = false, message = "Title is required." });
            }

            var path = Path.Combine(_env.ContentRootPath, _playlistFileName);
            var playlists = new List<PlaylistInfo>();

            if (System.IO.File.Exists(path))
            {
                var json = System.IO.File.ReadAllText(path);
                playlists = JsonSerializer.Deserialize<List<PlaylistInfo>>(json);
            }

            var newId = (playlists.Any() ? playlists.Max(p => int.Parse(p.Id)) + 1 : 1).ToString();

            var newPlaylist = new PlaylistInfo
            {
                Id = newId,
                Title = request.Title,
                CoverUrl = "/images/default-playlist.jpg",
                TrackCount = 0,
                Duration = "0:00",
                CreatedDate = DateTime.UtcNow
            };

            playlists.Add(newPlaylist);
            var updatedJson = JsonSerializer.Serialize(playlists, new JsonSerializerOptions { WriteIndented = true });
            System.IO.File.WriteAllText(path, updatedJson);

            return new JsonResult(new { success = true });
        }
    }

    public class CreatePlaylistRequest
    {
        public string Title { get; set; }
    }
} 