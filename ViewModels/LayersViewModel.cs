using NotesApp.Commands;
using NotesApp.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TileBasedLevelEditor.Interfaces;
using TileBasedLevelEditor.Models;
using TileBasedLevelEditor.Services;

namespace TileBasedLevelEditor.ViewModels
{
    public class LayersViewModel : ViewModelBase
    {
        private readonly ITilemapLayersParent _parent;
        private ICustomNavigationService _navigationService;

        private Layer? _selectedLayer;
        public Layer? SelectedLayer
        {
            get => _selectedLayer;
            set
            {
                _selectedLayer = value;
                OnPropertyChanged(nameof(SelectedLayer));
                OnPropertyChanged(nameof(CanDeleteLayer));
            }
        }
        public Tilemap CurrentTilemap => _parent.CurrentTilemap;

        private List<Layer> _layers => CurrentTilemap.Layers;
        public ObservableCollection<Layer> Layers => new ObservableCollection<Layer>(_layers);
        public bool CanDeleteLayer => SelectedLayer != null && _layers.Count > 1;

        public ICommand AddLayerCommand { get; }
        public ICommand DeleteLayerCommand { get; }

        public LayersViewModel(ITilemapLayersParent parent, ICustomNavigationService navigationService)
        {
            _parent = parent;
            _navigationService = navigationService;

            AddLayerCommand = new RelayCommand(AddLayer);
            DeleteLayerCommand = new RelayCommand(DeleteLayer);
        }

        private void AddLayer(object? parameter)
        {
            int newLayerIndex = _layers.Count;
            if (SelectedLayer != null)
                newLayerIndex = _layers.IndexOf(SelectedLayer) + 1;

            Layer newLayer = new Layer("New Layer");
            _layers.Insert(newLayerIndex, newLayer);

            SelectedLayer = newLayer;
            OnPropertyChanged(nameof(SelectedLayer));
            OnPropertyChanged(nameof(Layers));
        }

        private void DeleteLayer(object? parameter)
        {
            int layerIndex = _layers.IndexOf(SelectedLayer);

            if (!_layers.Remove(SelectedLayer))
                return;

            if (layerIndex >= _layers.Count)
                --layerIndex;

            SelectedLayer = _layers[layerIndex];

            OnPropertyChanged(nameof(SelectedLayer));
            OnPropertyChanged(nameof(Layers));
        }
    }
}
