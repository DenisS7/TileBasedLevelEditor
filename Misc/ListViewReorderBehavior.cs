using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TileBasedLevelEditor.Commands;
using TileBasedLevelEditor.CustomArgs;
using TileBasedLevelEditor.Models;
using TileBasedLevelEditor.ViewModels;

namespace TileBasedLevelEditor.Misc
{
	public class ListViewReorderBehavior
	{
		static LayerViewModel? _selectedItem = null;

		public static readonly DependencyProperty ReorderCommandProperty =
			DependencyProperty.RegisterAttached(
				"ReorderCommand",
				typeof(bool),
				typeof(ListViewReorderBehavior),
				new PropertyMetadata(default(bool), OnReorderCommandChanged)
				);

		public static void SetReorderCommand(DependencyObject d, bool cmd) => 
			d.SetValue(ReorderCommandProperty, cmd);

		public static bool GetReorderCommand(DependencyObject d) => 
			(bool)d.GetValue(ReorderCommandProperty);

		private static void OnReorderCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is not ListView listView || e.NewValue is not bool newValue)
				return;

			if (newValue)
			{
				listView.PreviewMouseLeftButtonDown += ListView_MouseLeftButtonDown;
				listView.MouseMove += ListView_MouseMove;
				listView.PreviewMouseLeftButtonUp += ListView_MouseLeftButtonUp;
			}
			else
			{
				listView.PreviewMouseLeftButtonDown -= ListView_MouseLeftButtonDown;
				listView.MouseMove -= ListView_MouseMove;
				listView.PreviewMouseLeftButtonUp -= ListView_MouseLeftButtonUp;
			}
		}

		private static LayerViewModel? GetItemAt(ListView listView, Point point, List<int>? validColumns = null)
		{
			HitTestResult result = VisualTreeHelper.HitTest(listView, point);
			if (result == null)
				return null;

			DependencyObject? hit = result.VisualHit;
			if (hit == null)
				return null;

			bool validHit = false;
			//TO DO: change this to variables in behavior
			if (validColumns != null)
			{
				if (listView.View is not GridView gridView || gridView.Columns.Count < 1)
					  return null;

				double prevX = 0;
				double nextX = 0;
				for (int i = 0; i < gridView.Columns.Count; i++)
				{
					nextX += gridView.Columns[i].Width;
					if (validColumns.Contains(i) && prevX < point.X && point.X < nextX)
					{
						validHit = true;
						break;
					}

					prevX = nextX;
				}
			}
			else
			{
				validHit = true;
			}

			if (!validHit)
				return null;

			DependencyObject? obj = VisualTreeHelper.GetParent(hit);
			while (obj != null)
			{
				if (obj is ListViewItem item2)
				{
					if (item2.DataContext is LayerViewModel vm)
						return vm;

					return null;
				}

				obj = VisualTreeHelper.GetParent(obj);
			}

			return null;
		}

		private static void ListView_MouseLeftButtonDown(object sender, MouseEventArgs e)
		{
			if (sender is not ListView listView)
				return;

			_selectedItem = GetItemAt(listView, e.GetPosition(listView), new List<int> { 0 });
			if (_selectedItem == null)
				return;

			listView.SelectedItem = _selectedItem;
			listView.CaptureMouse();
		}

		private static void ListView_MouseMove(object sender, MouseEventArgs e)
		{
			if (sender is not ListView listView)
				return;

			if (_selectedItem == null)
				return;

			LayersViewModel? vm2 = listView.DataContext as LayersViewModel;
			if (vm2 is null)
				return;

			var item0 = listView.Items.GetItemAt(0);
			var item1 = listView.Items.GetItemAt(1);

			LayerViewModel? item = GetItemAt(listView, e.GetPosition(listView));
			if (item is null)
				return;

			if (listView.SelectedItem == item)
				return;

			int from = listView.Items.IndexOf(_selectedItem);
			int to = item != null ? listView.Items.IndexOf(item) : listView.Items.Count;

			//change to command
			vm2.MoveLayerTo(from, to);
		}

		private static void ListView_MouseLeftButtonUp(object sender, MouseEventArgs e)
		{
			if (sender is not ListView listView)
				return;

			_selectedItem = null;
			//listView.ReleaseMouseCapture();
		}
	}
}
