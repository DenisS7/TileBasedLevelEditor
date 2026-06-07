using TileBasedLevelEditor.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TileBasedLevelEditor.Models;
using TileBasedLevelEditor.Serialization;
using TileBasedLevelEditor.Services;
using TileBasedLevelEditor.Interfaces;

namespace TileBasedLevelEditor.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ICustomNavigationService _navigationService;
        private ITilesetsService _tilesetsService;
        private ITilemapRendererService _tilemapRendererService;

        public TilesetViewModel TilesetViewModel { get; private set; }
        public TilemapEditorViewModel TilemapViewModel { get; private set; }
        public LayersViewModel LayersViewModel => TilemapViewModel.LayersViewModel;
        public MainWindowViewModel(ICustomNavigationService navigationService, ITilesetsService tilesetsService, ITilemapRendererService tilemapRendererService) 
        { 
            _navigationService = navigationService;
            _tilesetsService = tilesetsService;
            _tilemapRendererService = tilemapRendererService;
            TilesetViewModel = new TilesetViewModel(_navigationService, tilesetsService);
            TilemapViewModel = new TilemapEditorViewModel(_navigationService, tilesetsService, _tilemapRendererService);
        }
    }
}
