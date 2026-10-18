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
using System.Windows.Data;
using System.ComponentModel;
using Newtonsoft.Json.Bson;
using System.Collections.Specialized;

namespace TileBasedLevelEditor.ViewModels
{
	public class LayersViewModel : ViewModelBase
	{
		private ITilemapParent _parent;
		private ICustomNavigationService _navigationService;
		public Tilemap CurrentTilemap => _parent.CurrentTilemap;

		private LayerViewModel? _selectedLayer;
		public LayerViewModel? SelectedLayer
		{
			get => _selectedLayer;
			set
			{
				if (_selectedLayer != null && _selectedLayer.EditableName.IsEditing)
					_selectedLayer.EditableName.ApplyEditCommand.Execute(null);

				_selectedLayer = value;
				OnPropertyChanged(nameof(SelectedLayer));
				OnPropertyChanged(nameof(CanDeleteLayer));
			}
		}

		public ObservableCollection<LayerViewModel> AllLayers => _parent.Layers;
		private CollectionViewSource _layersSource;
		public ICollectionView Layers => _layersSource.View;
		public bool CanDeleteLayer => SelectedLayer != null && AllLayers.Count - 1 > 1;
		public bool CanReorderLayers => !AllLayers.Any(layer => layer.EditableName.IsEditing);

		public ICommand AddLayerCommand { get; }
		public ICommand DeleteLayerCommand { get; }
		public ICommand ChangeLayerVisibilityCommand { get; }
		public ICommand LockLayerCommand { get; }
		public ICommand SwapLayersCommand { get; }

		public LayersViewModel(ITilemapParent parent, ICustomNavigationService navigationService)
		{
			_parent = parent;
			_navigationService = navigationService;

			_layersSource = new CollectionViewSource { Source = AllLayers };
			_layersSource.View.Filter = layer => !ReferenceEquals(layer, AllLayers.Last());
			_parent.Layers.CollectionChanged += RefreshLayersSource;

			foreach (var layer in AllLayers)
			{
				AddLayerDependencies(layer);
			}

			AddLayerCommand = new RelayCommand(AddLayer);
			DeleteLayerCommand = new RelayCommand(DeleteLayer);
			ChangeLayerVisibilityCommand = new RelayCommand(ChangeLayerVisibility);
			LockLayerCommand = new RelayCommand(LockLayer);
			SwapLayersCommand = new RelayCommand(SwapLayers);
		}

		private void AddLayerDependencies(LayerViewModel layer)
		{
			layer.EditableName.PropertyChanged += (sender, e) =>
			{
				if (e.PropertyName == nameof(EditableTextViewModel.IsEditing))
				{
					OnPropertyChanged(nameof(CanReorderLayers));
				}
			};
		}

		public void SwapLayers(object? parameter)
		{
			if (parameter is not Tuple<int, int> values)
				return;

			AllLayers.Move(values.Item1, values.Item2);
			_parent.OnLayersSwapped(values.Item1, values.Item2);
		}

        private void RefreshLayersSource(object? sender, NotifyCollectionChangedEventArgs args)
        {
            _layersSource.View.Refresh();
        }

		private void AddLayer(object? parameter)
		{
			int newLayerIndex = AllLayers.Count - 1;
			if (SelectedLayer != null)
				newLayerIndex = AllLayers.IndexOf(SelectedLayer) + 1;

			CurrentTilemap.Layers.Insert(newLayerIndex, new Layer("New Layer", CurrentTilemap.TilemapSize.X * CurrentTilemap.TilemapSize.Y, Math.Max(newLayerIndex, AllLayers.Count - 1)));
			LayerViewModel newLayer = new LayerViewModel(CurrentTilemap.Layers[newLayerIndex]);
			for(int i = newLayer.VisibilityIndex + 1; i < AllLayers.Count - 1; i++)
			{
				++AllLayers[i].VisibilityIndex;
			}
			AllLayers.Insert(newLayerIndex, newLayer);
			AddLayerDependencies(newLayer);

			SelectedLayer = newLayer;
		}

		private void DeleteLayer(object? parameter)
		{
			if (parameter is not LayerViewModel deletedLayer)
				return;

			int layerIndex = AllLayers.IndexOf(deletedLayer);

			if (!AllLayers.Remove(deletedLayer))
				return;

			if (layerIndex >= AllLayers.Count - 1)
				--layerIndex;

			_parent.OnLayerDeleted(deletedLayer);
			SelectedLayer = AllLayers[layerIndex];
		}

		private void ChangeLayerVisibility(object? parameter)
		{
			if (parameter is not LayerViewModel layer)
				return;

			layer.Visible = !layer.Visible;
		}

		private void LockLayer(object? parameter)
		{
			if (parameter is not LayerViewModel layer)
				return;

			layer.Locked = !layer.Locked;
		}
	}
}
