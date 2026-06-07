using TileBasedLevelEditor.Commands;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TileBasedLevelEditor.CustomArgs;
using TileBasedLevelEditor.Models;
using TileBasedLevelEditor.ViewModels;

namespace TileBasedLevelEditor.Misc
{
    public static class TileSelector
    {
        public static bool IsDragging = false;

        public static readonly DependencyProperty HoverCommandProperty =
            DependencyProperty.RegisterAttached(
                "HoverCommand",
                typeof(CommandBase),
                typeof(TileSelector),
                new PropertyMetadata(null, OnHoverCommandChanged)
                );

        public static Vec2<int>? GetHoveredTile(Grid grid, Point mousePosition)
        {
            if (grid.DataContext is not TileGridViewModel vm)
                return null;

            Vec2<int> tilesMargin = vm.TileMargin;
            Vec2<int> tileSize = vm.TileSize;

            if (tileSize <= 0)
                return null;
            
            if (vm.ShouldBeCentered)
            {
                mousePosition.X -= (grid.ActualWidth - vm.CanvasWidth * vm.ScrollViewerZoom) / 2.0;
                mousePosition.Y -= (grid.ActualHeight - vm.CanvasHeight * vm.ScrollViewerZoom) / 2.0;
            }

            Vec2<int> hoveredTile = new Vec2<int>((int)(mousePosition.X / ((tileSize.X + tilesMargin.X) * vm.ScrollViewerZoom)), (int)(mousePosition.Y / ((tileSize.Y + tilesMargin.Y) * vm.ScrollViewerZoom)));
            //System.Diagnostics.Debug.WriteLine($"HoveredTile: {hoveredTile.X} {hoveredTile.Y}");

            if (hoveredTile >= vm.NrTiles)
                return null;

            return hoveredTile;
        }

        public static void SetHoverCommand(DependencyObject d, CommandBase cmd) =>
            d.SetValue(HoverCommandProperty, cmd);

        public static CommandBase GetHoverCommand(DependencyObject d) =>
            (CommandBase)d.GetValue(HoverCommandProperty);

        private static void OnHoverCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not Grid grid) 
                return;

            if (e.OldValue == null && e.NewValue != null)
            { 
                grid.MouseMove += Grid_MouseMove;
                grid.MouseLeave += Grid_MouseLeave;
            }
            else if (e.OldValue != null && e.NewValue == null)
            {
                grid.MouseMove -= Grid_MouseMove;
                grid.MouseLeave -= Grid_MouseLeave;
            }
        }

        private static void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            Grid grid = (Grid)sender;
            CommandBase cmd = GetHoverCommand(grid);

            if (cmd == null)
                return;

            if (grid.DataContext is not TileGridViewModel vm)
            {
                cmd.Execute(null);
                return;
            }

            cmd.Execute(GetHoveredTile(grid, e.GetPosition(grid)));
        }

        private static void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            Grid grid = (Grid)sender;
            CommandBase cmd = GetHoverCommand(grid);
            cmd?.Execute(null);
        }

        public static readonly DependencyProperty SelectCommandProperty =
            DependencyProperty.RegisterAttached(
                "SelectCommand",
                typeof(CommandBase),
                typeof(TileSelector),
                new PropertyMetadata(null, OnSelectCommandChanged)
            );

        public static void SetSelectCommand(DependencyObject d, CommandBase cmd) =>
            d.SetValue(SelectCommandProperty, cmd);

        public static CommandBase GetSelectCommand(DependencyObject d) =>
            (CommandBase)d.GetValue(SelectCommandProperty);

        private static void OnSelectCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not Grid grid)
                return;

            if (e.OldValue == null && e.NewValue != null)
            {
                grid.MouseLeftButtonDown += Grid_MouseLeftButtonDown;
                grid.MouseMove += Grid_MouseDragMove;
                grid.MouseLeftButtonUp += Grid_MouseLeftButtonUp;
            }
            else if (e.OldValue != null && e.NewValue == null)
            {
                grid.MouseLeftButtonDown -= Grid_MouseLeftButtonDown;
                grid.MouseMove -= Grid_MouseDragMove;
                grid.MouseLeftButtonUp -= Grid_MouseLeftButtonUp;
            }
        }

        private static void Grid_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            if (sender is not Grid grid)
                return;

            CommandBase cmd = GetSelectCommand(grid);

            if (cmd == null)
                return;

            grid.CaptureMouse();
            bool isAdd = (Keyboard.Modifiers & ModifierKeys.Control) != 0;

            if (grid.DataContext is TileGridViewModel vm)
            {
                IsDragging = true;

                Vec2<int>? hoveredTile = GetHoveredTile(grid, e.GetPosition(grid));
                //if (hoveredTile != null)
                cmd.Execute(new TileSelectionArgs(hoveredTile, isAdd, DragStage.Start));
            }
        }

        private static void Grid_MouseDragMove(object sender, MouseEventArgs e)
        {
            if (!IsDragging)
                return;

            if (sender is not Grid grid)
                return;

            CommandBase cmd = GetSelectCommand(grid);

            if (cmd == null)
                return;

            bool isAdd = (Keyboard.Modifiers & ModifierKeys.Control) != 0;
            if (grid.DataContext is TileGridViewModel vm)
            {
                Vec2<int>? hoveredTile = GetHoveredTile(grid, e.GetPosition(grid));
                if (hoveredTile != null)
                {
                    cmd.Execute(new TileSelectionArgs(hoveredTile, isAdd, DragStage.Dragging));
                }
            }
        }

        private static void Grid_MouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            if (!IsDragging)
                return;

            if (sender is not Grid grid)
                return;

            grid.ReleaseMouseCapture();
            CommandBase cmd = GetSelectCommand(grid);

            if (cmd == null)
                return;

            bool isAdd = (Keyboard.Modifiers & ModifierKeys.Control) != 0;
            if (grid.DataContext is TileGridViewModel vm)
            {
                IsDragging = false;

                Vec2<int>? hoveredTile = GetHoveredTile(grid, e.GetPosition(grid));
                if (hoveredTile != null)
                {
                    cmd.Execute(new TileSelectionArgs(hoveredTile, isAdd, DragStage.End));
                }
            }
        }
    }
}
