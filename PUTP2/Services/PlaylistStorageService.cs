using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using PUTP2.Models;

namespace PUTP2.Services
{
    public class PlaylistStorageService
    {
        private readonly string _dataDirectory;
        private readonly string _playlistsFile;
        private readonly string _tracksFile;

        public PlaylistStorageService()
        {
            _dataDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Data");
            _playlistsFile = Path.Combine(_dataDirectory, "playlists.json");
            _tracksFile = Path.Combine(_dataDirectory, "tracks.json");

            // Create data directory if it doesn't exist
            if (!Directory.Exists(_dataDirectory))
            {
                Directory.CreateDirectory(_dataDirectory);
            }
        }

        public List<PlaylistInfo> LoadPlaylists()
        {
            if (!File.Exists(_playlistsFile))
            {
                return new List<PlaylistInfo>();
            }

            var json = File.ReadAllText(_playlistsFile);
            return JsonSerializer.Deserialize<List<PlaylistInfo>>(json) ?? new List<PlaylistInfo>();
        }

        public List<Track> LoadTracks()
        {
            if (!File.Exists(_tracksFile))
            {
                return new List<Track>();
            }

            var json = File.ReadAllText(_tracksFile);
            return JsonSerializer.Deserialize<List<Track>>(json) ?? new List<Track>();
        }

        public void SavePlaylists(List<PlaylistInfo> playlists)
        {
            var json = JsonSerializer.Serialize(playlists, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            File.WriteAllText(_playlistsFile, json);
        }

        public void SaveTracks(List<Track> tracks)
        {
            var json = JsonSerializer.Serialize(tracks, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            File.WriteAllText(_tracksFile, json);
        }
    }
} 