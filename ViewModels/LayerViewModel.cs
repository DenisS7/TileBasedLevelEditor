using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TileBasedLevelEditor.Commands;
using TileBasedLevelEditor.Interfaces;
using TileBasedLevelEditor.Models;
using TileBasedLevelEditor.Services;
using TileBasedLevelEditor.ViewModel;

namespace TileBasedLevelEditor.ViewModels
{
	public class LayerViewModel : ViewModelBase
	{
		private Layer _layer;
		public Layer Layer
		{
			get => _layer;
			set
			{
				_layer = value;
				OnPropertyChanged(nameof(Layer));
			}
		}

		public EditableTextViewModel EditableName { get; }
		public string Name
		{
			get => Layer.Name;
			set
			{
				Layer.Name = value;
				OnPropertyChanged(nameof(Name));
			}
		}

		public bool Visible
		{
			get => Layer.Visible;
			set
			{
				Layer.Visible = value;
				OnPropertyChanged(nameof(Visible));
			}
		}

		public int VisibilityIndex
		{
			get => Layer.VisibilityIndex;
			set
			{
				Layer.VisibilityIndex = value;
				OnPropertyChanged(nameof(VisibilityIndex));
			}
		}

		public TileData[] Tiles
		{
			get => Layer.Tiles;
			set
			{
				Layer.Tiles = value;
				OnPropertyChanged(nameof(Tiles));
			}
		}

		public double Opacity
		{
			get => Layer.Opacity;
			set
			{
				Layer.Opacity = value;
				OnPropertyChanged(nameof(Opacity));
			}
		}

		public bool Locked
		{
			get => Layer.Locked;
			set
			{
				Layer.Locked = value;
				OnPropertyChanged(nameof(Locked));
			}
		}

		public LayerViewModel(Layer layer)
		{
			_layer = layer;
			EditableName = new EditableTextViewModel(layer.Name, OnRename);
		}

		public void OnRename(string newName)
		{
			Name = newName;
		}
	}
}
