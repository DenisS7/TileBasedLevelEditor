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
using TileBasedLevelEditor.Interfaces;

namespace TileBasedLevelEditor.ViewModels
{
    public class TilemapEditorViewModel : ViewModelBase, ITilemapLayersParent
    {
        private ICustomNavigationService _navigationService;
        private ITilesetsService _tilesetsService;
        
        private LayersViewModel _layersViewModel;
        public LayersViewModel LayersViewModel => _layersViewModel;

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
        private Layer? SelectedLayer => _layersViewModel.SelectedLayer;

        public event Action? RequestCloseNewTilemapDialog;
        public event Action? RequestCloseEditTilemapDialog;

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
            _layersViewModel = new LayersViewModel(this, _navigationService);
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
            _layersViewModel = new LayersViewModel(this, _navigationService);
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

            foreach (Layer layer in CurrentTilemap.Layers)
            {
                TileData[] Copy = (TileData[])layer.Tiles.Clone();

                layer.Tiles = new TileData[newTilemapArraySize];
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
                        layer.Tiles[newTileArrayIndex] = Copy[oldTileArrayIndex];
                        Tileset tileset = _tilesetsService.Tilesets[Copy[oldTileArrayIndex].TilesetID];
                        TilemapImages[newTileArrayIndex] = tileset.TileImages[Copy[oldTileArrayIndex].TilesetIndex.X + tileset.NrTiles.X * Copy[oldTileArrayIndex].TilesetIndex.Y];
                    }
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

        private List<Layer> GetTopLayersForTileAt(int tileArrayIndex, int n = 1)
        {
            List<Layer> topLayers = new List<Layer>();
            int currentLayerPosition = 0;
            foreach (Layer layer in CurrentTilemap.Layers)
            {
                if (layer.Tiles[tileArrayIndex].TilesetID != Guid.Empty)
                {
                    topLayers.Add(layer);
                    currentLayerPosition++;
                    if (currentLayerPosition >= n)
                        return topLayers;
                }
            }

            return topLayers;
        }

        private void ClearPreviousHoveredTiles()
        {
            foreach (Tuple<Vec2<int>, CroppedBitmap?> previousHovered in HoveredOverTiles)
            {
                Vec2<int> tilemapTileIndex = previousHovered.Item1;
                TileGridVM.TileImages[tilemapTileIndex.X + tilemapTileIndex.Y * TilemapSize.X] = previousHovered.Item2;
            }

            HoveredOverTiles.Clear();
        }

        private void OnTileHovered(Vec2<int>? vec)
        {
            if (TileSelectedService.SelectedTiles == null || TileSelectedService.SelectedTiles.Count == 0)
                return;

            ClearPreviousHoveredTiles();

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

            if (SelectedLayer == null)
            {
                _layersViewModel.SelectedLayer = CurrentTilemap.Layers.Last();
            }

            ClearPreviousHoveredTiles();

            TileData referenceTile = TileSelectedService.SelectedTiles[0].Item1;

            foreach (Tuple<TileData, CroppedBitmap?> tileData in TileSelectedService.SelectedTiles)
            {
                Vec2<int> tilemapTileIndex = vec + tileData.Item1.TilesetIndex - referenceTile.TilesetIndex;
                if (tilemapTileIndex < 0 || tilemapTileIndex >= TilemapSize)
                    continue;

                CurrentTilemap.SetTile(tilemapTileIndex, tileData.Item1.TilesetIndex, SelectedLayer, tileData.Item1.TilesetID);
                int tilemapTileArrayIndex = tilemapTileIndex.X + tilemapTileIndex.Y * TilemapSize.X;
                List<Layer> topLayers = GetTopLayersForTileAt(tilemapTileArrayIndex);
                if (topLayers.Count == 0 || topLayers[0] == SelectedLayer)
                {
                    TileGridVM.TileImages[tilemapTileArrayIndex] = tileData.Item2;
                }
            }
            HoveredOverTiles.Clear();
        }

        public void OnLayerDeleted(Layer layer)
        {
            for(int i = 0; i < layer.Tiles.Length; i++)
            {
                if (layer.Tiles[i].TilesetID == Guid.Empty)
                    continue;

                List<Layer> topLayers = GetTopLayersForTileAt(i);

                if(topLayers.Count == 0)
                {
                    TileGridVM.TileImages[i] = null;
                    continue;
                }

                if (topLayers[0].VisibilityIndex < layer.VisibilityIndex)
                    continue;

                Tileset tileset = _tilesetsService.Tilesets[topLayers[0].Tiles[i].TilesetID];
                int tilesetTileArrayIndex = topLayers[0].Tiles[i].TilesetIndex.X + tileset.NrTiles.X * topLayers[0].Tiles[i].TilesetIndex.Y;
                TileGridVM.TileImages[i] = tileset.TileImages[tilesetTileArrayIndex];
            }
        }
        public void OnLayerVisibilityChange(Layer layer)
        {
            if (layer.Visible)
            {
                for (int i = 0; i < layer.Tiles.Length; i++)
                {
                    if (layer.Tiles[i].TilesetID == Guid.Empty)
                        continue;

                    List<Layer> topLayers = GetTopLayersForTileAt(i);

                    if (topLayers.Count == 0 || topLayers[0] != layer)
                        continue;

                    Tileset tileset = _tilesetsService.Tilesets[topLayers[0].Tiles[i].TilesetID];
                    int tilesetTileArrayIndex = topLayers[0].Tiles[i].TilesetIndex.X + tileset.NrTiles.X * topLayers[0].Tiles[i].TilesetIndex.Y;
                    TileGridVM.TileImages[i] = tileset.TileImages[tilesetTileArrayIndex];
                }
            }
            else
            {
                for (int i = 0; i < layer.Tiles.Length; i++)
                {
                    if (layer.Tiles[i].TilesetID == Guid.Empty)
                        continue;

                    List<Layer> topLayers = GetTopLayersForTileAt(i, 2);

                    if (topLayers.Count > 0 && topLayers[0] != layer)
                        continue;

                    if (topLayers.Count == 1)
                    {
                        TileGridVM.TileImages[i] = null;
                        continue;
                    }

                    Tileset tileset = _tilesetsService.Tilesets[topLayers[1].Tiles[i].TilesetID];
                    int tilesetTileArrayIndex = topLayers[1].Tiles[i].TilesetIndex.X + tileset.NrTiles.X * topLayers[1].Tiles[i].TilesetIndex.Y;
                    TileGridVM.TileImages[i] = tileset.TileImages[tilesetTileArrayIndex];
                }
            }
        }
    }
}
