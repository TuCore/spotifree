using Microsoft.VisualBasic;
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
        private bool _hasAlbums;
        public event Action RequestNavigateToSettings;
        public ObservableCollection<AlbumViewModel> Albums { get; }
        public bool HasAlbums
        {
            get => _hasAlbums;
            set => SetProperty(ref _hasAlbums, value);
        }

        public ICommand SelectAlbumCommand { get; }
        public ICommand GoToSettingsCommand { get; }
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
            GoToSettingsCommand = new RelayCommand(_ => RequestNavigateToSettings?.Invoke());

        }
        private void ExecuteSelectAlbum(object? param)
        {
            if (param is AlbumViewModel album)
            {
                _mainViewModel.NavigateToAlbumDetail(album);
            }
        }

        private async void ExecuteRenameAlbum(object? param)
        {
            if (param is AlbumViewModel album)
            {
                // Hiện hộp thoại nhập tên từ thư viện VisualBasic
                string newName = Interaction.InputBox(
                    $"Nhập tên mới cho album '{album.Name}':",
                    "Đổi tên Album",
                    album.Name
                );

                // Nếu người dùng nhập gì đó và khác tên cũ
                if (!string.IsNullOrWhiteSpace(newName) && newName != album.Name)
                {
                    // Gọi Service đổi tên file
                    await _libraryService.UpdateAlbumNameAsync(album.Name, album.Artist, newName);
                }
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

            var renameCommand = new RelayCommand(ExecuteRenameAlbum);

            var groupedByAlbum = tracks
                .GroupBy(t => new { AlbumName = t.Album ?? "Unknown Album", ArtistName = t.Artist ?? "Unknown Artist" })
                .Select(g => new AlbumViewModel(
                    g.Key.AlbumName,
                    g.Key.ArtistName,
                    LoadImageFromBytes(g.First().CoverArt),
                    new ObservableCollection<LocalTrack>(g.ToList()),
                    renameCommand
                ));

            foreach (var album in groupedByAlbum)
            {
                Albums.Add(album);
            }
            HasAlbums = Albums.Any();
        }

        private void OnLibraryChanged()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(LoadAlbums);
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