using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;

public class UploadModel : PageModel
{
    private readonly string uploadsPath = Path.Combine("Data", "UploadedSongs");
    private readonly string tracksJsonPath = Path.Combine("Data", "tracks.json");
    private readonly string playlistsPath = Path.Combine("Data", "playlists.json");
    private readonly string playlistCoversPath = Path.Combine("wwwroot", "images", "playlist-covers");

    public void OnGet()
    {
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

            // Create directories if they don't exist
            Directory.CreateDirectory(uploadsPath);

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}.mp3";
            var filePath = Path.Combine(uploadsPath, fileName);

            // Save the file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Update tracks.json
            var trackInfo = new TrackInfo
            {
                Id = Guid.NewGuid().ToString(),
                Name = trackName,
                Artist = artist,
                FilePath = filePath,
                UploadDate = DateTime.Now
            };

            var tracks = new List<TrackInfo>();
            if (System.IO.File.Exists(tracksJsonPath))
            {
                var json = await System.IO.File.ReadAllTextAsync(tracksJsonPath);
                tracks = JsonSerializer.Deserialize<List<TrackInfo>>(json) ?? new List<TrackInfo>();
            }

            tracks.Add(trackInfo);
            await System.IO.File.WriteAllTextAsync(tracksJsonPath, 
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

            // Create directories if they don't exist
            Directory.CreateDirectory(uploadsPath);

            // Generate unique filename
            var fileName = $"recording_{Guid.NewGuid()}.mp3";
            var filePath = Path.Combine(uploadsPath, fileName);

            // Save the recording
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Create track info
            var trackId = Guid.NewGuid().ToString();
            var trackInfo = new TrackInfo
            {
                Id = trackId,
                Name = trackName,
                FilePath = filePath,
                UploadDate = DateTime.Now,
                IsRecording = true
            };

            // Update tracks.json
            var tracks = new List<TrackInfo>();
            if (System.IO.File.Exists(tracksJsonPath))
            {
                var json = await System.IO.File.ReadAllTextAsync(tracksJsonPath);
                tracks = JsonSerializer.Deserialize<List<TrackInfo>>(json) ?? new List<TrackInfo>();
            }
            tracks.Add(trackInfo);
            await System.IO.File.WriteAllTextAsync(tracksJsonPath, 
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
                await System.IO.File.WriteAllTextAsync(playlistsPath, 
                    JsonSerializer.Serialize(playlists, new JsonSerializerOptions { WriteIndented = true }));
            }

            return new JsonResult(new { success = true });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, message = ex.Message });
        }
    }

    public JsonResult OnPostCreatePlaylist([FromForm] string title, IFormFile coverFile = null)
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

            // Read existing playlists
            List<PlaylistInfo> playlists;
            if (System.IO.File.Exists(playlistsPath))
            {
                var json = System.IO.File.ReadAllText(playlistsPath);
                playlists = JsonSerializer.Deserialize<List<PlaylistInfo>>(json) ?? new List<PlaylistInfo>();
            }
            else
            {
                playlists = new List<PlaylistInfo>();
            }

            string newId = Guid.NewGuid().ToString();

            var newPlaylist = new PlaylistInfo
            {
                Id = newId,
                Title = title,
                CoverUrl = coverUrl,
                Tracks = new List<string>(),
                CreatedDate = DateTime.Now,
                TrackCount = 0,
                Duration = "0:00",
                Action = $"Playlist{newId}"
            };

            playlists.Add(newPlaylist);

            // Save updated playlists
            var jsonOutput = JsonSerializer.Serialize(playlists, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            System.IO.File.WriteAllText(playlistsPath, jsonOutput);

            return new JsonResult(new { success = true, playlist = newPlaylist });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, message = ex.Message });
        }
    }
}

public class TrackInfo
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Artist { get; set; }
    public string FilePath { get; set; }
    public DateTime UploadDate { get; set; }
    public bool IsRecording { get; set; }
}

public class PlaylistInfo
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string CoverUrl { get; set; }
    public List<string> Tracks { get; set; }
    public int TrackCount { get; set; }
    public string Duration { get; set; }
    public string Action { get; set; }
    public DateTime CreatedDate { get; set; }

    public PlaylistInfo()
    {
        Tracks = new List<string>();
    }
} 