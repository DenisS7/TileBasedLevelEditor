using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NotesApp.Commands;
using System.Windows.Input;
using System.Windows.Shapes;
using NotesApp.ViewModel;
using TileBasedLevelEditor.Models;
using TileBasedLevelEditor.Services;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using System.Windows.Navigation;
using System.Windows;
using TileBasedLevelEditor.Serialization;
using Newtonsoft.Json.Bson;

namespace TileBasedLevelEditor.ViewModels
{
    public class TilemapEditorViewModel : ViewModelBase
    {
        private ICustomNavigationService _navigationService;
        private ITilesetsService _tilesetsService;

        private Tilemap _currentTilemap;

        public Tilemap CurrentTilemap
        {
            get => _currentTilemap;
            set
            {
                _currentTilemap = value;
                //TO DO use tilemap tiles after serialization is done
                TileGridVM.SetNewGridValues(CurrentTilemap.TileSize, CurrentTilemap.TilemapSize, null);

                OnPropertyChanged(nameof(CurrentTilemap));
            }
        }

        private string _newTilemapName = "";
        public string NewTilemapName
        {
            get => _newTilemapName;
            set
            {
                _newTilemapName = value;
                OnPropertyChanged(nameof(NewTilemapName));
            }
        }

        private string _newTilemapTileWidth = "";
        public string NewTilemapTileWidth
        {
            get => _newTilemapTileWidth;
            set
            {
                _newTilemapTileWidth = value;
                OnPropertyChanged(nameof(NewTilemapTileWidth));
            }
        }

        private string _newTilemapTileHeight = "";
        public string NewTilemapTileHeight
        {
            get => _newTilemapTileHeight;
            set
            {
                _newTilemapTileHeight = value;
                OnPropertyChanged(nameof(NewTilemapTileHeight));
            }
        }

        private string _newTilemapWidth = "";
        public string NewTilemapWidth
        {
            get => _newTilemapWidth;
            set
            {
                _newTilemapWidth = value;
                OnPropertyChanged(nameof(NewTilemapWidth));
            }
        }

        private string _newTilemapHeight = "";
        public string NewTilemapHeight
        {
            get => _newTilemapHeight;
            set
            {
                _newTilemapHeight = value;
                OnPropertyChanged(nameof(NewTilemapHeight));
            }
        }

        private Vec2<int> TilemapSize => CurrentTilemap.TilemapSize;
        public Vec2<int> TileSize => CurrentTilemap.TileSize;

        public event Action? RequestCloseNewTilemapDialog;
        public event Action? RequestCloseEditTilemapDialog;

        public ObservableCollection<Layer> Layers => new ObservableCollection<Layer>(CurrentTilemap.Layers);
        public TileGridViewModel TileGridVM { get; }
        public List<Tuple<Vec2<int>, CroppedBitmap?>> HoveredOverTiles = [];

        public ICommand CreateNewTilemapCommand { get; }
        public ICommand AddNewTilemapCommand { get; }
        public ICommand CancelNewTilemapCommand { get; }
        public ICommand EditCurrentTilemapCommand { get; }
        public ICommand SaveEditedCurrentTilemapCommand { get; }
        public ICommand CancelEditingTilemapCommand { get; }

        public TilemapEditorViewModel(ICustomNavigationService navigationService, ITilesetsService tilesetsService)
        {
            _navigationService = navigationService;
            _tilesetsService = tilesetsService;
            _currentTilemap = new Tilemap("TestTilemap", new Vec2<int>(32, 32), new Vec2<int>(20, 15));
            TileGridVM = new TileGridViewModel(TileSize, TilemapSize, new Vec2<int>(0, 0), null, OnTileHovered, OnTileSelected, true, true, false);

            CreateNewTilemapCommand = new RelayCommand(OnCreateNewTilemap);
            AddNewTilemapCommand = new RelayCommand(OnAddNewTilemap);
            CancelNewTilemapCommand = new RelayCommand(OnCancelNewTileset);
            EditCurrentTilemapCommand = new RelayCommand(OnEditCurrentTilemap);
            SaveEditedCurrentTilemapCommand = new RelayCommand(OnSaveEditedCurrentTilemap);
            CancelEditingTilemapCommand = new RelayCommand(OnCancelEditingTilemap);
        }

        public TilemapEditorViewModel(ICustomNavigationService navigationService, ITilesetsService tilesetsService, Tilemap currentTilemap)
        {
            _navigationService = navigationService;
            _tilesetsService = tilesetsService;
            _currentTilemap = currentTilemap;
            TileGridVM = new TileGridViewModel(TileSize, TilemapSize, new Vec2<int>(0, 0), null, OnTileHovered, OnTileSelected, true, true, false);

            CreateNewTilemapCommand = new RelayCommand(OnCreateNewTilemap);
            AddNewTilemapCommand = new RelayCommand(OnAddNewTilemap);
            CancelNewTilemapCommand = new RelayCommand(OnCancelNewTileset);
            EditCurrentTilemapCommand = new RelayCommand(OnEditCurrentTilemap);
            SaveEditedCurrentTilemapCommand = new RelayCommand(OnSaveEditedCurrentTilemap);
            CancelEditingTilemapCommand = new RelayCommand(OnCancelEditingTilemap);
        }

        private List<CroppedBitmap?>? EditTilemapSize(Vec2<int> newTilemapSize)
        {
            if (CurrentTilemap.TilesetsUsed.Count == 0)
                return null;

            int newTilemapArraySize = newTilemapSize.X * newTilemapSize.Y;
            List<CroppedBitmap?> TilemapImages = Enumerable.Repeat<CroppedBitmap?>(null, newTilemapArraySize).ToList();
            TileData[] Copy = (TileData[])CurrentTilemap.Tiles.Clone();
            
            CurrentTilemap.Tiles = new TileData[newTilemapArraySize];
            Vec2<int> UnusedTile = new Vec2<int>(-1);
            Vec2<int> MinTilemapSize = new Vec2<int>(Math.Min(CurrentTilemap.TilemapSize.X, newTilemapSize.X), Math.Min(CurrentTilemap.TilemapSize.Y, newTilemapSize.Y));
            for (int y = 0; y < MinTilemapSize.Y; y++)
            {
                for (int x = 0; x < MinTilemapSize.X; x++)
                {
                    int oldTileArrayIndex = x + y * CurrentTilemap.TilemapSize.X;

                    if (Copy[oldTileArrayIndex].TilesetIndex == null || Copy[oldTileArrayIndex].TilesetIndex == UnusedTile)
                        continue;

                    int newTileArrayIndex = x + y * newTilemapSize.X;
                    CurrentTilemap.Tiles[newTileArrayIndex] = Copy[oldTileArrayIndex];
                    Tileset tileset = _tilesetsService.Tilesets[Copy[oldTileArrayIndex].TilesetID];
                    TilemapImages[newTileArrayIndex] = tileset.TileImages[Copy[oldTileArrayIndex].TilesetIndex.X + tileset.NrTiles.X * Copy[oldTileArrayIndex].TilesetIndex.Y];
                }
            }

            return TilemapImages;
        }

        private void OnCreateNewTilemap(object? parameter)
        {
            _navigationService.OpenNewTilemapDialog(this);
        }

        private void OnAddNewTilemap(object? parameter)
        {
            if (NewTilemapName.Length < 3
                || NewTilemapTileHeight.Length == 0 || NewTilemapTileWidth.Length == 0 
                || NewTilemapWidth.Length == 0 || NewTilemapHeight.Length == 0)
                return;

            try
            {
                Vec2<int> NewTilemapTileSize = new Vec2<int>(Int32.Parse(NewTilemapTileWidth), Int32.Parse(NewTilemapTileHeight));
                Vec2<int> NewTilemapSize = new Vec2<int>(Int32.Parse(NewTilemapWidth), Int32.Parse(NewTilemapHeight));
                Tilemap newTilemap = new Tilemap(NewTilemapName, NewTilemapTileSize, NewTilemapSize);
                //TO DO after tilemap serialization is done
                //if (Serializer.SaveTilemap(newTilemap))
                {
                    //_tilemapsService.AddTileset(newTilemap);
                    //OnPropertyChanged(nameof(Tilemaps));
                    CurrentTilemap = newTilemap;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error creating tilemap:\n{ex.Message}",
                    "Load Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            RequestCloseNewTilemapDialog?.Invoke();
        }

        private void OnCancelNewTileset(object? parameter)
        {
            RequestCloseNewTilemapDialog?.Invoke();
        }

        private void OnEditCurrentTilemap(object? parameter)
        {
            _navigationService.OpenEditTilemapDialog(this);

            NewTilemapName = CurrentTilemap.Name;
            NewTilemapTileWidth = CurrentTilemap.TileSize.X.ToString();
            NewTilemapTileHeight = CurrentTilemap.TileSize.Y.ToString();
            NewTilemapWidth = CurrentTilemap.TilemapSize.X.ToString();
            NewTilemapHeight = CurrentTilemap.TilemapSize.Y.ToString();
        }

        private void OnSaveEditedCurrentTilemap(object? parameter)
        {
            if (NewTilemapName.Length < 3
                || NewTilemapTileHeight.Length == 0 || NewTilemapTileWidth.Length == 0
                || NewTilemapWidth.Length == 0 || NewTilemapHeight.Length == 0)
                return;

            Vec2<int> NewTilemapTileSize = new Vec2<int>(Int32.Parse(NewTilemapTileWidth), Int32.Parse(NewTilemapTileHeight));
            Vec2<int> NewTilemapSize = new Vec2<int>(Int32.Parse(NewTilemapWidth), Int32.Parse(NewTilemapHeight));

            List<CroppedBitmap?>? NewTileImages = EditTilemapSize(NewTilemapSize);

            CurrentTilemap.Name = NewTilemapName;
            CurrentTilemap.TileSize = NewTilemapTileSize;
            CurrentTilemap.TilemapSize = NewTilemapSize;

            TileGridVM.SetNewGridValues(NewTilemapTileSize, NewTilemapSize, NewTileImages);

            RequestCloseEditTilemapDialog?.Invoke();
        }

        private void OnCancelEditingTilemap(object? parameter)
        {
            RequestCloseEditTilemapDialog?.Invoke();
        }

        private void OnTileHovered(Vec2<int>? vec)
        {
            if (TileSelectedService.SelectedTiles == null || TileSelectedService.SelectedTiles.Count == 0)
                return;

            foreach (Tuple<Vec2<int>, CroppedBitmap?> previousHovered in HoveredOverTiles)
            {
                Vec2<int> tilemapTileIndex = previousHovered.Item1;
                TileGridVM.TileImages[tilemapTileIndex.X + tilemapTileIndex.Y * TilemapSize.X] = previousHovered.Item2;
            }

            HoveredOverTiles.Clear();

            if (vec == null)
                return;

            TileData referenceTile = TileSelectedService.SelectedTiles[0].Item1;

            foreach (Tuple<TileData, CroppedBitmap?> tileData in TileSelectedService.SelectedTiles)
            {
                Vec2<int> tilemapTileIndex = vec + tileData.Item1.TilesetIndex - referenceTile.TilesetIndex;
                if (tilemapTileIndex < 0 || tilemapTileIndex >= TilemapSize)
                    continue;
                
                HoveredOverTiles.Add(new Tuple<Vec2<int>, CroppedBitmap?>(tilemapTileIndex, TileGridVM.TileImages[tilemapTileIndex.X + tilemapTileIndex.Y * TilemapSize.X]));
                TileGridVM.TileImages[tilemapTileIndex.X + tilemapTileIndex.Y * TilemapSize.X] = tileData.Item2;
            }
        }

        private void OnTileSelected(Vec2<int>? vec)
        {
            if (TileSelectedService.SelectedTiles == null || TileSelectedService.SelectedTiles.Count == 0 || vec == null)
                return;

            TileData referenceTile = TileSelectedService.SelectedTiles[0].Item1;

            foreach (Tuple<TileData, CroppedBitmap?> tileData in TileSelectedService.SelectedTiles)
            {
                Vec2<int> tilemapTileIndex = vec + tileData.Item1.TilesetIndex - referenceTile.TilesetIndex;
                if (tilemapTileIndex < 0 || tilemapTileIndex >= TilemapSize)
                    continue;

                CurrentTilemap.SetTile(tilemapTileIndex, tileData.Item1.TilesetIndex, tileData.Item1.TilesetID);
                TileGridVM.TileImages[tilemapTileIndex.X + tilemapTileIndex.Y * TilemapSize.X] = tileData.Item2;
            }
            HoveredOverTiles.Clear();
        }
    }
}
