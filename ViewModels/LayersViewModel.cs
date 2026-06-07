using TileBasedLevelEditor.Commands;
using TileBasedLevelEditor.ViewModel;
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
        private ITilemapLayersParent _parent;
        private ICustomNavigationService _navigationService;
        public Tilemap CurrentTilemap => _parent.CurrentTilemap;

        private LayerViewModel? _selectedLayer;
        public LayerViewModel? SelectedLayer
        {
            get => _selectedLayer;
            set
            {
                _selectedLayer = value;
                OnPropertyChanged(nameof(SelectedLayer));
                OnPropertyChanged(nameof(CanDeleteLayer));
            }
        }

        public ObservableCollection<LayerViewModel> Layers { get; }// => new ObservableCollection<Layer>(_layers);
        public bool CanDeleteLayer => SelectedLayer != null && Layers.Count > 1;

        public ICommand AddLayerCommand { get; }
        public ICommand DeleteLayerCommand { get; }
        public ICommand ChangeLayerVisibilityCommand { get; }

        public LayersViewModel(ITilemapLayersParent parent, ICustomNavigationService navigationService)
        {
            _parent = parent;
            _navigationService = navigationService;
            Layers = new ObservableCollection<LayerViewModel>();

            foreach (Layer layer in _parent.CurrentTilemap.Layers)
            {
                Layers.Add(new LayerViewModel(layer));
            }

            AddLayerCommand = new RelayCommand(AddLayer);
            DeleteLayerCommand = new RelayCommand(DeleteLayer);
            ChangeLayerVisibilityCommand = new RelayCommand(ChangeLayerVisibility);
        }

        private void AddLayer(object? parameter)
        {
            int newLayerIndex = Layers.Count;
            if (SelectedLayer != null)
                newLayerIndex = Layers.IndexOf(SelectedLayer) + 1;

            CurrentTilemap.Layers.Insert(newLayerIndex, new Layer("New Layer", CurrentTilemap.TilemapSize.X * CurrentTilemap.TilemapSize.Y, newLayerIndex - 1));
            LayerViewModel newLayer = new LayerViewModel(CurrentTilemap.Layers[newLayerIndex]);
            Layers.Insert(newLayerIndex, newLayer);
            for(int i = newLayer.VisibilityIndex + 1; i < Layers.Count; i++)
            {
                ++Layers[i].VisibilityIndex;
            }

            SelectedLayer = newLayer;
        }

        private void DeleteLayer(object? parameter)
        {
            if (parameter is not LayerViewModel deletedLayer)
                return;

            int layerIndex = Layers.IndexOf(deletedLayer);

            if (!Layers.Remove(deletedLayer))
                return;

            if (layerIndex >= Layers.Count)
                --layerIndex;

            _parent.OnLayerDeleted(deletedLayer);
            SelectedLayer = Layers[layerIndex];
        }

        private void ChangeLayerVisibility(object? parameter)
        {
            if (parameter is not LayerViewModel layer)
                return;

            layer.Visible = !layer.Visible;
        }
    }
}
