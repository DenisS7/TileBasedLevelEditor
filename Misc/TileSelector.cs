using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using NotesApp.Commands;
using TileBasedLevelEditor.CustomArgs;
using TileBasedLevelEditor.Models;
using TileBasedLevelEditor.ViewModels;
using TileBasedLevelEditor.Views;

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
        public static Vec2<int>? GetHoveredTile(Rectangle rectangle, Point mousePosition)
        {
            if (rectangle.DataContext is not TileGridViewModel vm)
                return null;

            Vec2<int> tilesMargin = vm.TileMargin;
            Vec2<int> tileSize = vm.TileSize;
            Vec2<int> rectangleSize = vm.NrTiles;

            if (tileSize <= 0)
                return null;

            Vec2<int> hoveredTile = new Vec2<int>((int)(mousePosition.X / (tileSize.X + tilesMargin.X)), (int)(mousePosition.Y / (tileSize.Y + tilesMargin.Y)));

            if (hoveredTile < 0 || hoveredTile >= vm.NrTiles)
                return null;

            return hoveredTile;
        }

        public static void SetHoverCommand(DependencyObject d, CommandBase cmd) =>
            d.SetValue(HoverCommandProperty, cmd);

        public static CommandBase GetHoverCommand(DependencyObject d) =>
            (CommandBase)d.GetValue(HoverCommandProperty);

        private static void OnHoverCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not Rectangle rectangle) 
                return;

            if (e.OldValue == null && e.NewValue != null)
            { 
                rectangle.MouseMove += Rectangle_MouseMove;
                rectangle.MouseLeave += Rectangle_MouseLeave;
            }
            else if (e.OldValue != null && e.NewValue == null)
            {
                rectangle.MouseMove -= Rectangle_MouseMove;
                rectangle.MouseLeave -= Rectangle_MouseLeave;
            }
        }

        private static void Rectangle_MouseMove(object sender, MouseEventArgs e)
        {
            Rectangle rectangle = (Rectangle)sender;
            CommandBase cmd = GetHoverCommand(rectangle);

            if (cmd == null)
                return;

            if (rectangle.DataContext is not TileGridViewModel vm)
            {
                cmd.Execute(null);
                return;
            }

            cmd.Execute(GetHoveredTile(rectangle, e.GetPosition(rectangle)));
        }

        private static void Rectangle_MouseLeave(object sender, MouseEventArgs e)
        {
            Rectangle rectangle = (Rectangle)sender;
            CommandBase cmd = GetHoverCommand(rectangle);
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
            if (d is not Rectangle rectangle)
                return;

            if (e.OldValue == null && e.NewValue != null)
            {
                rectangle.MouseLeftButtonDown += Rectangle_MouseLeftButtonDown;
                rectangle.MouseMove += Rectangle_MouseDragMove;
                rectangle.MouseLeftButtonUp += Rectangle_MouseLeftButtonUp;
            }
            else if (e.OldValue != null && e.NewValue == null)
            {
                rectangle.MouseLeftButtonDown -= Rectangle_MouseLeftButtonDown;
                rectangle.MouseMove -= Rectangle_MouseDragMove;
                rectangle.MouseLeftButtonUp -= Rectangle_MouseLeftButtonUp;
            }
        }

        private static void Rectangle_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            if (sender is not Rectangle rectangle)
                return;

            CommandBase cmd = GetSelectCommand(rectangle);

            if (cmd == null)
                return;

            rectangle.CaptureMouse();
            bool isAdd = (Keyboard.Modifiers & ModifierKeys.Control) != 0;

            if (rectangle.DataContext is TileGridViewModel vm)
            {
                IsDragging = true;

                Vec2<int>? hoveredTile = GetHoveredTile(rectangle, e.GetPosition(rectangle));
                //if (hoveredTile != null)
                cmd.Execute(new TileSelectionArgs(hoveredTile, isAdd, DragStage.Start));
            }
        }

        private static void Rectangle_MouseDragMove(object sender, MouseEventArgs e)
        {
            if (!IsDragging)
                return;

            if (sender is not Rectangle rectangle)
                return;

            CommandBase cmd = GetSelectCommand(rectangle);

            if (cmd == null)
                return;

            bool isAdd = (Keyboard.Modifiers & ModifierKeys.Control) != 0;
            if (rectangle.DataContext is TileGridViewModel vm)
            {
                Vec2<int>? hoveredTile = GetHoveredTile(rectangle, e.GetPosition(rectangle));
                if (hoveredTile != null)
                    cmd.Execute(new TileSelectionArgs(hoveredTile, isAdd, DragStage.Dragging));
            }
        }

        private static void Rectangle_MouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            if (!IsDragging)
                return;

            if (sender is not Rectangle rectangle)
                return;

            rectangle.ReleaseMouseCapture();
            CommandBase cmd = GetSelectCommand(rectangle);

            if (cmd == null)
                return;

            bool isAdd = (Keyboard.Modifiers & ModifierKeys.Control) != 0;
            if (rectangle.DataContext is TileGridViewModel vm)
            {
                IsDragging = false;

                Vec2<int>? hoveredTile = GetHoveredTile(rectangle, e.GetPosition(rectangle));
                if (hoveredTile != null)
                    cmd.Execute(new TileSelectionArgs(hoveredTile, isAdd, DragStage.End));
            }
        }
    }
}
