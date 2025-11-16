using Spotifree.IServices;
using Spotifree.Models;
using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace Spotifree.Services
{
    public class MusicLibraryService : IMusicLibraryService
    {
        private List<LocalTrack> _trackCache = new();

        private readonly ISettingsService _settingsService;

        public event Action? LibraryChanged;

        public MusicLibraryService(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public Task<IEnumerable<LocalTrack>> GetLibraryAsync()
        {
            return Task.FromResult(_trackCache.AsEnumerable());
        }

        public async Task ScanLibraryAsync()
        {
            var settings = await _settingsService.GetAsync();
            if (settings.MusicFolderPaths == null || !settings.MusicFolderPaths.Any())
            {
                _trackCache = new List<LocalTrack>();
                LibraryChanged?.Invoke(); 
                return;
            }

            var allTracksFound = new List<LocalTrack>();
            var allowedExtensions = new[] { ".mp3", ".m4a", ".flac", ".wav", ".ogg" };

            foreach (var folderPath in settings.MusicFolderPaths)
            {
                if (!Directory.Exists(folderPath))
                {
                    continue;
                }

                try
                {
                    var filesInFolder = Directory.EnumerateFiles(
                        folderPath, "*.*", SearchOption.AllDirectories
                    ).Where(file => allowedExtensions.Contains(Path.GetExtension(file).ToLowerInvariant()));

                    foreach (var filePath in filesInFolder)
                    {
                        try
                        {
                            using (var tagFile = TagLib.File.Create(filePath))
                            {
                                var tag = tagFile.Tag;
                                var track = new LocalTrack
                                {
                                    FilePath = filePath,
                                    Title = string.IsNullOrEmpty(tag.Title) ? Path.GetFileNameWithoutExtension(filePath) : tag.Title,
                                    Artist = string.IsNullOrEmpty(tag.FirstPerformer) ? "Unknown Artist" : tag.FirstPerformer,
                                    Album = string.IsNullOrEmpty(tag.Album) ? "Unknown Album" : tag.Album,

                                    Duration = tagFile.Properties.Duration.TotalSeconds,
                                    TrackNumber = tag.Track, 
                                    Year = tag.Year, 
                                    CoverArt = tag.Pictures.Length > 0 ? tag.Pictures[0].Data.Data : null
                                };
                                allTracksFound.Add(track);
                            }
                        }
                        catch (Exception)
                        {
                            // Skip file corrupt
                        }
                    }
                }
                catch (Exception)
                {
                    // Skip folder did not access
                }
            }
            _trackCache = allTracksFound;
            LibraryChanged?.Invoke();
        }
    }
}