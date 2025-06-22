using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;
using NotesApp.Commands;
using NotesApp.ViewModel;
using TileBasedLevelEditor.Models;
using TileBasedLevelEditor.Services;
using TileBasedLevelEditor.Views;

namespace TileBasedLevelEditor.ViewModels
{
    public class TilesetViewModel : ViewModelBase
    {
        private ICustomNavigationService _navigationService;
        private List<Tileset> _tilesets = [];
        private Tileset? _currentTileset;

        public Tileset? CurrentTileset
        {
            get => _currentTileset;
            private set
            {
                if (_currentTileset == value) return;
                _currentTileset = value;
                OnPropertyChanged(nameof(CurrentTileset));

                if (_currentTileset != null)
                {
                    GetTilesetImage();
                    OnPropertyChanged(nameof(ImageSize));
                    OnPropertyChanged(nameof(TileSize));
                    OnPropertyChanged(nameof(NrTiles));
                }
                else
                {
                    TilesetImage = null;
                    OnPropertyChanged(nameof(ImageSize));
                    OnPropertyChanged(nameof(TileSize));
                    OnPropertyChanged(nameof(NrTiles));
                }
            }
        }

        private string _newTilesetName = "";
        public string NewTilesetName
        {
            get => _newTilesetName;
            set
            {
                _newTilesetName = value;
                OnPropertyChanged(nameof(NewTilesetName));
            }
        }

        private string _newTilesetPath = "";
        public string NewTilesetPath
        {
            get => _newTilesetPath;
            set
            {
                _newTilesetPath = value;
                OnPropertyChanged(nameof(NewTilesetPath));
            }
        }

        private string _newTilesetTileWidth = "";
        public string NewTilesetTileWidth
        {
            get => _newTilesetTileWidth;
            set
            {
                _newTilesetTileWidth = value;
                OnPropertyChanged(nameof(NewTilesetTileWidth));
            }
        }

        private string _newTilesetTileHeight = "";
        public string NewTilesetTileHeight
        {
            get => _newTilesetTileHeight;
            set
            {
                _newTilesetTileHeight = value;
                OnPropertyChanged(nameof(NewTilesetTileHeight));
            }
        }

        public Vec2<int> TileSize => CurrentTileset?.TileSize ?? new Vec2<int>(0);

        public Vec2<int> ImageSize => CurrentTileset?.ImageSize ?? new Vec2<int>(0);

        public Vec2<int> NrTiles => CurrentTileset?.NrTiles ?? new Vec2<int>(0);

        private ImageSource? _tilesetImage;

        public ImageSource? TilesetImage
        {
            get => _tilesetImage;
            private set
            {
                if (_tilesetImage == value) return;
                _tilesetImage = value;
                OnPropertyChanged(nameof(TilesetImage));
            }
        }

        public event Action? RequestCloseNewTilesetDialog;
        public ICommand CreateNewTilesetCommand { get; }
        public ICommand AddNewTilesetCommand { get; }
        public ICommand CancelNewTilesetCommand { get; }
        public ICommand ChooseTilesetImageCommand { get; }

        public TileGridViewModel TileGridVM { get; }

        public TilesetViewModel(ICustomNavigationService navigationService)
        {
            _navigationService = navigationService;
            _currentTileset = null;
            CreateNewTilesetCommand = new RelayCommand(OnCreateNewTileset);
            AddNewTilesetCommand = new RelayCommand(OnAddNewTileset);
            ChooseTilesetImageCommand = new RelayCommand(OnChooseTilesetImage);
            CancelNewTilesetCommand = new RelayCommand(OnCancelNewTileset);
            TileGridVM = new TileGridViewModel(TileSize, NrTiles, new Vec2<int>(2, 2), null, OnTileSelected);
        }

        public TilesetViewModel(Tileset currentTileset, ICustomNavigationService navigationService)
        {
            _navigationService = navigationService;
            _currentTileset = currentTileset;
            GetTilesetImage();
            CreateNewTilesetCommand = new RelayCommand(OnCreateNewTileset);
            AddNewTilesetCommand = new RelayCommand(OnAddNewTileset);
            ChooseTilesetImageCommand = new RelayCommand(OnChooseTilesetImage);
            CancelNewTilesetCommand = new RelayCommand(OnCancelNewTileset);
            TileGridVM = new TileGridViewModel(TileSize, NrTiles, new Vec2<int>(2, 2));
            ChooseTilesetImageCommand = new RelayCommand(OnChooseTilesetImage);
        }

        private void OnCreateNewTileset(object? parameter)
        {
            _navigationService.OpenNewTilesetDialog(this);
        }

        private void OnAddNewTileset(object? parameter)
        {
            if (NewTilesetName.Length < 3 || NewTilesetPath.Length == 0 ||
                NewTilesetTileHeight.Length == 0 || NewTilesetTileWidth.Length == 0)
                return;

            try
            {
                Vec2<int> NewTilesetTileSize = new Vec2<int>(Int32.Parse(NewTilesetTileWidth), Int32.Parse(NewTilesetTileHeight));
                CurrentTileset = new Tileset(NewTilesetName, NewTilesetTileSize, NewTilesetPath);
                TileGridVM.TileSize = TileSize;
                TileGridVM.NrTiles = NrTiles;
                _tilesets.Add(CurrentTileset);
                RequestCloseNewTilesetDialog?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error loading tileset:\n{ex.Message}",
                    "Load Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void OnCancelNewTileset(object? parameter)
        {
            RequestCloseNewTilesetDialog?.Invoke();
        }

        private void OnChooseTilesetImage(object? parameter)
        {
            var dlg = new OpenFileDialog
            {
                Title = "Select a Tileset Image",
                Filter = "PNG Files (*.png)|*.png|JPEG Files (*.jpg;*.jpeg)|*.jpg;*.jpeg|All Images|*.png;*.jpg;*.jpeg"
            };

            bool? result = dlg.ShowDialog();
            if (result != true) 
                return;

            string path = dlg.FileName;
            if (!File.Exists(path)) 
                return;

            NewTilesetPath = path;
            //NewTilesetPath = System.IO.Path.GetFileNameWithoutExtension(path);
            //
            ////hardcoded size for now
            //Vec2<int> tileSize = new Vec2<int>(32, 32);
            //
            //try
            //{
            //    CurrentTileset = new Tileset(name, tileSize, path);
            //    TileGridVM.TileSize = tileSize;
            //    TileGridVM.NrTiles = NrTiles;
            //    _tilesets.Add(CurrentTileset);
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(
            //        $"Error loading tileset:\n{ex.Message}",
            //        "Load Error",
            //        MessageBoxButton.OK,
            //        MessageBoxImage.Error);
            //}
        }

        private void GetTilesetImage()
        {
            if (CurrentTileset?.ImageData == null)
            {
                TilesetImage = null;
                return;
            }

            TileGridVM.TileImages.Clear();
            try
            {
                var bmp = new BitmapImage();
                using (var ms = new MemoryStream(CurrentTileset.ImageData))
                {
                    bmp.BeginInit();
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.StreamSource = ms;
                    bmp.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                    bmp.EndInit();
                    bmp.Freeze();
                }
                TilesetImage = bmp;

                for (int y = 0; y < NrTiles.Y; y++)
                {
                    for (int x = 0; x < NrTiles.X; x++)
                    {
                        var rect = new Int32Rect(
                            x * TileSize.X,
                            y * TileSize.Y,
                            TileSize.X,
                            TileSize.Y
                        );
                        var tileBmp = new CroppedBitmap(bmp, rect);
                        TileGridVM.TileImages.Add(tileBmp);
                    }
                }
            }
            catch
            {
                TilesetImage = null;
            }
        }

        private void OnTileSelected(Vec2<int>? vec)
        {
            if (CurrentTileset == null || TileGridVM.InitialSelectedTile == null)
                return;

            if (TileGridVM.InitialSelectedTile < 0 || TileGridVM.InitialSelectedTile >= NrTiles)
                return;

            Debug.WriteLine("NEW /n /n /n");
            HashSet<Vec2<int>> SelectedTiles = [];
            List<Tuple<TileData, CroppedBitmap?>> SelectedTilesFull = [];
            foreach (SelectionArea selectionArea in TileGridVM.SelectionAreas)
            {
                for (int i = selectionArea.StartTile.X; i <= selectionArea.EndTile.X; i++)
                {
                    for (int j = selectionArea.StartTile.Y; j <= selectionArea.EndTile.Y; j++)
                    {
                        if(SelectedTiles.Add(new Vec2<int>(i, j)))
                        {
                            CroppedBitmap? tileImage = TileGridVM.TileImages[SelectedTiles.Last().X + SelectedTiles.Last().Y * NrTiles.X];
                            SelectedTilesFull.Add(new Tuple<TileData, CroppedBitmap?>(new TileData(SelectedTiles.Last(), CurrentTileset.Name), tileImage));
                        }
                        
                    }
                }
            }
            TileSelectedService.SelectedTiles = SelectedTilesFull;
        }
    }
}
