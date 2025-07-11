using NotesApp.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TileBasedLevelEditor.Models;
using TileBasedLevelEditor.Serialization;
using TileBasedLevelEditor.Services;

namespace TileBasedLevelEditor.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ICustomNavigationService _navigationService;
        private ITilesetsService _tilesetsService;

        public TilesetViewModel TilesetViewModel { get; private set; }
        public MainWindowViewModel(ICustomNavigationService navigationService, TilesetsService tilesetsService) 
        { 
            _navigationService = navigationService;
            _tilesetsService = tilesetsService;
            TilesetViewModel = new TilesetViewModel(_navigationService, tilesetsService);
        }
    }
}
