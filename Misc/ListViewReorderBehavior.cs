using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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
		static object? _selectedItem = null;

		public static readonly DependencyProperty IsEnabledProperty =
			DependencyProperty.RegisterAttached(
				"IsEnabled",
				typeof(bool),
				typeof(ListViewReorderBehavior),
				new PropertyMetadata(default(bool), OnReorderCommandChanged)
				);

		public static void SetIsEnabled(DependencyObject d, bool value) => 
			d.SetValue(IsEnabledProperty, value);

		public static bool GetIsEnabled(DependencyObject d) => 
			(bool)d.GetValue(IsEnabledProperty);

		public static readonly DependencyProperty ColumnsToApplyProperty =
			DependencyProperty.RegisterAttached(
				"ColumnsToApply",
				typeof(string),
				typeof(ListViewReorderBehavior),
				new PropertyMetadata(null)
				);
		public static void SetColumnsToApply(DependencyObject d, string value) => 
			d.SetValue(ColumnsToApplyProperty, value);

		public static string GetColumnsToApply(DependencyObject d) => 
			(string)d.GetValue(ColumnsToApplyProperty);

		public static readonly DependencyProperty ItemViewModelTypeProperty =
			DependencyProperty.RegisterAttached(
				"ItemViewModelType",
				typeof(Type),
				typeof(ListViewReorderBehavior),
				new PropertyMetadata(null)
				);

		public static void SetItemViewModelType(DependencyObject d, Type value) =>
			d.SetValue(ItemViewModelTypeProperty, value);

		public static Type GetItemViewModelType(DependencyObject d) =>
			(Type)d.GetValue(ItemViewModelTypeProperty);

		public static readonly DependencyProperty CommandOnReorderProperty =
			DependencyProperty.RegisterAttached(
				"CommandOnReorder",
				typeof(ICommand),
				typeof(ListViewReorderBehavior),
				new PropertyMetadata(null)
				);

		public static void SetCommandOnReorder(DependencyObject d, ICommand cmd) =>
			d.SetValue(CommandOnReorderProperty, cmd);

		public static ICommand GetCommandOnReorder(DependencyObject d) =>
			(ICommand)d.GetValue(CommandOnReorderProperty);

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

		private static T? GetItemAtGeneric<T>(ListView listView, Point point, bool takeColumnsIntoAccount = false) where T : class
		{
			HitTestResult result = VisualTreeHelper.HitTest(listView, point);
			if (result == null)
				return null;

			DependencyObject? hit = result.VisualHit;
			if (hit == null)
				return null;

			List<int>? validColumns = GetColumnsToApply(listView)?.Split(',').Select(int.Parse).ToList();

			bool validHit = false;
			if (takeColumnsIntoAccount && validColumns != null)
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
					if (item2.DataContext is T vm)
						return vm;

					return null;
				}

				obj = VisualTreeHelper.GetParent(obj);
			}

			return null;
		}

		private static object? GetItemAt(Type itemType, ListView listView, Point point, bool takeColumnsIntoAccount = false)
		{
			MethodInfo? method = typeof(ListViewReorderBehavior).GetMethod(nameof(GetItemAtGeneric), BindingFlags.Static | BindingFlags.NonPublic)?.MakeGenericMethod(itemType);
			return method?.Invoke(typeof(ListViewReorderBehavior), [listView, point, takeColumnsIntoAccount]);
		}

		private static void ListView_MouseLeftButtonDown(object sender, MouseEventArgs e)
		{
			if (sender is not ListView listView)
				return;

			_selectedItem = GetItemAt(GetItemViewModelType(listView), listView, e.GetPosition(listView), true);
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

			var item0 = listView.Items.GetItemAt(0);
			var item1 = listView.Items.GetItemAt(1);

			object? item = GetItemAt(GetItemViewModelType(listView), listView, e.GetPosition(listView));
			if (item == null)
				return;

			if (listView.SelectedItem == item)
				return;

			int from = listView.Items.IndexOf(_selectedItem);
			int to = item != null ? listView.Items.IndexOf(item) : listView.Items.Count;

			GetCommandOnReorder(listView)?.Execute(new Tuple<int, int>(from, to));
		}

		private static void ListView_MouseLeftButtonUp(object sender, MouseEventArgs e)
		{
			if (sender is not ListView listView)
				return;

			_selectedItem = null;
			listView.ReleaseMouseCapture();
		}
	}
}
