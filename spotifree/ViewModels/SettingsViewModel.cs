using Microsoft.Win32;
using Spotifree.IServices;
using Spotifree.Models;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using System.Windows.Input;

namespace Spotifree.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        private readonly ISettingsService _settingsService;
        private readonly IMusicLibraryService _libraryService;
        private readonly IThemeService _themeService;
        private readonly MainViewModel _mainViewModel;

        private AppSettings _settings = new AppSettings();
        private string _selectedFolderPath = string.Empty;
        private bool _isScanning;

        public bool IsScanning
        {
            get => _isScanning;
            set
            {
                if (SetProperty(ref _isScanning, value))
                {
                    ((RelayCommand)RescanLibraryCommand).RaiseCanExecuteChanged();
                }
            }
        }

        // Gets the collection of music folder paths to display in the ListView.
        public ObservableCollection<string> MusicFolderPaths { get; }

        // Gets or sets the currently selected folder path from the ListView.
        public string SelectedFolderPath
        {
            get => _selectedFolderPath;
            set
            {
                if (SetProperty(ref _selectedFolderPath, value))
                {
                    // Tell the "Remove" command to re-evaluate.
                    ((RelayCommand)RemoveFolderCommand).RaiseCanExecuteChanged();
                }
            }
        }

        /// A "proxy" property for the Dark Mode toggle.
        /// It reads from and writes to the loaded _settings object.
        public bool IsDarkTheme
        {
            get => _settings?.IsDarkTheme ?? false;
            set
            {
                if (_settings == null || _settings.IsDarkTheme == value) return;
                _settings.IsDarkTheme = value;
                OnPropertyChanged(nameof(IsDarkTheme));
                _themeService.SetTheme(value);
                _settingsService.SaveAsync(_settings);
                _mainViewModel.NavigateTo(this);
            }
        }

        public ICommand SelectFolderCommand { get; }
        public ICommand RemoveFolderCommand { get; }
        public ICommand RescanLibraryCommand { get; }

        public SettingsViewModel(
            ISettingsService settingsService,
            IMusicLibraryService libraryService,
            IThemeService themeService,
            MainViewModel mainViewModel)
        {
            // Inject dependencies
            _settingsService = settingsService;
            _libraryService = libraryService;
            _themeService = themeService;
            _mainViewModel = mainViewModel;

            // Initialize UI state
            MusicFolderPaths = new ObservableCollection<string>();

            SelectFolderCommand = new RelayCommand(async (_) => await ExecuteSelectFolderAsync());
            RemoveFolderCommand = new RelayCommand(async (_) => await ExecuteRemoveFolderAsync(), (_) => CanExecuteRemoveFolder(_));
            RescanLibraryCommand = new RelayCommand(async (_) => await ExecuteRescanLibraryAsync(), (_) => CanExecuteRescan(_));

            LoadSettingsAsync();
        }


        /// Asynchronously loads settings from the service on startup.
        private async void LoadSettingsAsync()
        {
            try
            {
                _settings = await _settingsService.GetAsync();
                MusicFolderPaths.Clear();
                foreach (var path in _settings.MusicFolderPaths)
                {
                    MusicFolderPaths.Add(path);
                }
                OnPropertyChanged(nameof(IsDarkTheme));
            }
            catch (Exception)
            {
                _settings = new AppSettings();
            }
        }

        /// Shows a folder dialog and adds the selected path.
        private async Task ExecuteSelectFolderAsync()
        {
            var dialog = new FolderBrowserDialog
            {
                Description = "Select a music folder to add",
                UseDescriptionForTitle = true,
                ShowNewFolderButton = true
            };

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            string newPath = dialog.SelectedPath;

            if (string.IsNullOrEmpty(newPath) || MusicFolderPaths.Contains(newPath))
            {
                return;
            }

            await _settingsService.AddMusicFolderAsync(newPath);
            MusicFolderPaths.Add(newPath);
        }

        // Removes the 'SelectedFolderPath' from the list and settings.
        private async Task ExecuteRemoveFolderAsync()
        {
            var pathToRemove = SelectedFolderPath;
            if (string.IsNullOrEmpty(pathToRemove)) return;
            await _settingsService.RemoveMusicFolderAsync(pathToRemove);
            MusicFolderPaths.Remove(pathToRemove);
        }

        private bool CanExecuteRemoveFolder(object? _)
        {
            return !string.IsNullOrEmpty(SelectedFolderPath);
        }

        // Triggers a full library rescan via the service.
        private async Task ExecuteRescanLibraryAsync()
        {
            IsScanning = true;
            try
            {
                await _libraryService.ScanLibraryAsync();
                //show scan fail
            }
            catch (Exception)
            {
                //show scan fail
            }
            finally
            {
                IsScanning = false;
            }
        }

        /// Checks if the "Rescan Library" button should be enabled.
        private bool CanExecuteRescan(object? _)
        {
            return !IsScanning;
        }
    }
}