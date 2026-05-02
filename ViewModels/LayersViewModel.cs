using NotesApp.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TileBasedLevelEditor.Interfaces;
using TileBasedLevelEditor.Models;
using TileBasedLevelEditor.Services;

namespace TileBasedLevelEditor.ViewModels
{
    public class LayersViewModel : ViewModelBase
    {
        private readonly ITilemapLayersParent _parent;
        private ICustomNavigationService _navigationService;
        public ObservableCollection<Layer> Layers => new ObservableCollection<Layer>(_parent.CurrentTilemap.Layers);

        public LayersViewModel(ITilemapLayersParent parent, ICustomNavigationService navigationService)
        {
            _parent = parent;
            _navigationService = navigationService;
        }
    }
}
