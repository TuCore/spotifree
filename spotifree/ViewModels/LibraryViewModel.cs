using Spotifree.IServices;
using Spotifree.Models;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Spotifree.ViewModels
{
    public class LibraryViewModel : BaseViewModel
    {
        private readonly IMusicLibraryService _libraryService;
        private readonly IAudioPlayerService _player;
        private readonly MainViewModel _mainViewModel;

        public ObservableCollection<AlbumViewModel> Albums { get; }

        public ICommand SelectAlbumCommand { get; }
        public LibraryViewModel(
            IMusicLibraryService library,
            IAudioPlayerService player,
            MainViewModel mainViewModel)
        {
            _libraryService = library;
            _player = player;
            _mainViewModel = mainViewModel;
            Albums = new ObservableCollection<AlbumViewModel>();

            _libraryService.LibraryChanged += OnLibraryChanged;
            LoadAlbums();
            SelectAlbumCommand = new RelayCommand(ExecuteSelectAlbum);

        }
        private void ExecuteSelectAlbum(object? param)
        {
            if (param is AlbumViewModel album)
            {
                _mainViewModel.NavigateToAlbumDetail(album);
            }
        }

        private async void LoadAlbums()
        {
            Albums.Clear();
            var tracks = await _libraryService.GetLibraryAsync();

            if (tracks == null || !tracks.Any())
            {
                return;
            }

            var groupedByAlbum = tracks
                .GroupBy(t => new { AlbumName = t.Album ?? "Unknown Album", ArtistName = t.Artist ?? "Unknown Artist" })
                .Select(g => new AlbumViewModel(
                    g.Key.AlbumName,
                    g.Key.ArtistName,
                    LoadImageFromBytes(g.First().CoverArt),
                    new ObservableCollection<LocalTrack>(g.ToList())
                ));

            foreach (var album in groupedByAlbum)
            {
                Albums.Add(album);
            }
        }

        private void OnLibraryChanged()
        {
            LoadAlbums();
        }

        private BitmapImage? LoadImageFromBytes(byte[]? imageData)
        {
            if (imageData == null || imageData.Length == 0)
                return null;

            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }
    }
}