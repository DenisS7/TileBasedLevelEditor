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

namespace TileBasedLevelEditor.ViewModels
{
    public class TilemapEditorViewModel : ViewModelBase
    {
        private ICustomNavigationService _navigationService;

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

        public ObservableCollection<Layer> Layers => new ObservableCollection<Layer>(CurrentTilemap.Layers);
        public TileGridViewModel TileGridVM { get; }
        public List<Tuple<Vec2<int>, CroppedBitmap?>> HoveredOverTiles = [];

        public ICommand CreateNewTilemapCommand { get; }
        public ICommand AddNewTilemapCommand { get; }
        public ICommand CancelNewTilemapCommand { get; }

        public TilemapEditorViewModel(ICustomNavigationService navigationService)
        {
            _navigationService = navigationService;
            _currentTilemap = new Tilemap("TestTilemap", new Vec2<int>(32, 32), new Vec2<int>(20, 15));
            TileGridVM = new TileGridViewModel(TileSize, TilemapSize, new Vec2<int>(0, 0), null, OnTileHovered, OnTileSelected, true, true, false);

            CreateNewTilemapCommand = new RelayCommand(OnCreateNewTilemap);
            AddNewTilemapCommand = new RelayCommand(OnAddNewTilemap);
            CancelNewTilemapCommand = new RelayCommand(OnCancelNewTileset);
        }

        public TilemapEditorViewModel(ICustomNavigationService navigationService, Tilemap currentTilemap)
        {
            _navigationService = navigationService;
            _currentTilemap = currentTilemap;
            TileGridVM = new TileGridViewModel(TileSize, TilemapSize, new Vec2<int>(0, 0), null, OnTileHovered, OnTileSelected, true, true, false);

            CreateNewTilemapCommand = new RelayCommand(OnCreateNewTilemap);
            AddNewTilemapCommand = new RelayCommand(OnAddNewTilemap);
            CancelNewTilemapCommand = new RelayCommand(OnCancelNewTileset);
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

                CurrentTilemap.SetTile(tilemapTileIndex, tileData.Item1.TilesetIndex, tileData.Item1.TilesetName);
                TileGridVM.TileImages[tilemapTileIndex.X + tilemapTileIndex.Y * TilemapSize.X] = tileData.Item2;
            }
            HoveredOverTiles.Clear();
        }
    }
}
