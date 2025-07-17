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
using TileBasedLevelEditor.CustomArgs;
using TileBasedLevelEditor.Models;
using TileBasedLevelEditor.Serialization;
using TileBasedLevelEditor.Services;
using TileBasedLevelEditor.Views;

namespace TileBasedLevelEditor.ViewModels
{
    public class TilesetViewModel : ViewModelBase
    {
        private ICustomNavigationService _navigationService;
        private TilesetsService _tilesetsService;
        public ObservableCollection<Tileset> Tilesets => _tilesetsService.Tilesets;
        private Tileset? _currentTileset;

        public Tileset? CurrentTileset
        {
            get => _currentTileset;
            set
            {
                if (_currentTileset == value) 
                    return;

                _currentTileset = value;
                OnPropertyChanged(nameof(CurrentTileset));
                OnPropertyChanged(nameof(ImageSize));
                OnPropertyChanged(nameof(TileSize));
                OnPropertyChanged(nameof(NrTiles));

                if (_currentTileset != null)
                {
                    TileGridVM.TileImages = new ObservableCollection<CroppedBitmap?>(_currentTileset.TileImages);
                    TileGridVM.TileSize = TileSize;
                    TileGridVM.NrTiles = NrTiles;
                    TileGridVM.IsTileHovered = false;
                    TileGridVM.SelectTileCommand.Execute(new TileSelectionArgs(null, false, DragStage.Start));
                     
                    //TO DO
                    //TileGridVM.TileMargin = ;
                    //TileGridVM.ImagePadding = ;
                }
                else
                {
                    TileGridVM.TileImages.Clear();
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

        public event Action? RequestCloseNewTilesetDialog;
        public ICommand CreateNewTilesetCommand { get; }
        public ICommand AddNewTilesetCommand { get; }
        public ICommand CancelNewTilesetCommand { get; }
        public ICommand ChooseTilesetImageCommand { get; }

        public TileGridViewModel TileGridVM { get; }

        public TilesetViewModel(ICustomNavigationService navigationService, TilesetsService tilesetsService)
        {
            _navigationService = navigationService;
            _tilesetsService = tilesetsService;
            if (_tilesetsService.Tilesets != null && _tilesetsService.Tilesets.Count > 0)
            {                
                _currentTileset = _tilesetsService.Tilesets[0];
            }
            else
            {
                _currentTileset = null;
            }
            TileGridVM = new TileGridViewModel(TileSize, NrTiles, new Vec2<int>(2, 2), CurrentTileset?.TileImages, OnTileSelected);
            CreateNewTilesetCommand = new RelayCommand(OnCreateNewTileset);
            AddNewTilesetCommand = new RelayCommand(OnAddNewTileset);
            ChooseTilesetImageCommand = new RelayCommand(OnChooseTilesetImage);
            CancelNewTilesetCommand = new RelayCommand(OnCancelNewTileset);
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
                Tileset newTileset = new Tileset(NewTilesetName, NewTilesetTileSize, NewTilesetPath);
                if (Serializer.SaveTileset(newTileset))
                {    
                    _tilesetsService.AddTileset(newTileset);
                    OnPropertyChanged(nameof(Tilesets));
                    CurrentTileset = newTileset;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error loading tileset:\n{ex.Message}",
                    "Load Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            RequestCloseNewTilesetDialog?.Invoke();
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

        private void OnTileSelected(Vec2<int>? vec)
        {
            if (CurrentTileset == null || TileGridVM.InitialSelectedTile == null)
                return;

            if (TileGridVM.InitialSelectedTile < 0 || TileGridVM.InitialSelectedTile >= NrTiles)
                return;

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
