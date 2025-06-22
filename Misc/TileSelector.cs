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
        public static Vec2<int>? GetHoveredTile(Canvas canvas, Point mousePosition)
        {
            if (canvas.DataContext is not TileGridViewModel vm)
                return null;

            Vec2<int> tilesMargin = vm.TileMargin;
            Vec2<int> tileSize = vm.TileSize;

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
            if (d is not Canvas canvas) 
                return;

            if (e.OldValue == null && e.NewValue != null)
            { 
                canvas.MouseMove += Canvas_MouseMove;
                canvas.MouseLeave += Canvas_MouseLeave;
            }
            else if (e.OldValue != null && e.NewValue == null)
            {
                canvas.MouseMove -= Canvas_MouseMove;
                canvas.MouseLeave -= Canvas_MouseLeave;
            }
        }

        private static void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            Canvas canvas = (Canvas)sender;
            CommandBase cmd = GetHoverCommand(canvas);

            if (cmd == null)
                return;

            if (canvas.DataContext is not TileGridViewModel vm)
            {
                cmd.Execute(null);
                return;
            }

            cmd.Execute(GetHoveredTile(canvas, e.GetPosition(canvas)));
        }

        private static void Canvas_MouseLeave(object sender, MouseEventArgs e)
        {
            Canvas canvas = (Canvas)sender;
            CommandBase cmd = GetHoverCommand(canvas);
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
            if (d is not Canvas canvas)
                return;

            if (e.OldValue == null && e.NewValue != null)
            {
                canvas.MouseLeftButtonDown += Canvas_MouseLeftButtonDown;
                canvas.MouseMove += Canvas_MouseDragMove;
                canvas.MouseLeftButtonUp += Canvas_MouseLeftButtonUp;
            }
            else if (e.OldValue != null && e.NewValue == null)
            {
                canvas.MouseLeftButtonDown -= Canvas_MouseLeftButtonDown;
                canvas.MouseMove -= Canvas_MouseDragMove;
                canvas.MouseLeftButtonUp -= Canvas_MouseLeftButtonUp;
            }
        }

        private static void Canvas_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            if (sender is not Canvas canvas)
                return;

            CommandBase cmd = GetSelectCommand(canvas);

            if (cmd == null)
                return;

            canvas.CaptureMouse();
            bool isAdd = (Keyboard.Modifiers & ModifierKeys.Control) != 0;

            if (canvas.DataContext is TileGridViewModel vm)
            {
                IsDragging = true;

                Vec2<int>? hoveredTile = GetHoveredTile(canvas, e.GetPosition(canvas));
                //if (hoveredTile != null)
                cmd.Execute(new TileSelectionArgs(hoveredTile, isAdd, DragStage.Start));
            }
        }

        private static void Canvas_MouseDragMove(object sender, MouseEventArgs e)
        {
            if (!IsDragging)
                return;

            if (sender is not Canvas canvas)
                return;

            CommandBase cmd = GetSelectCommand(canvas);

            if (cmd == null)
                return;

            bool isAdd = (Keyboard.Modifiers & ModifierKeys.Control) != 0;
            if (canvas.DataContext is TileGridViewModel vm)
            {
                Vec2<int>? hoveredTile = GetHoveredTile(canvas, e.GetPosition(canvas));
                if (hoveredTile != null)
                    cmd.Execute(new TileSelectionArgs(hoveredTile, isAdd, DragStage.Dragging));
            }
        }

        private static void Canvas_MouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            if (!IsDragging)
                return;

            if (sender is not Canvas canvas)
                return;

            canvas.ReleaseMouseCapture();
            CommandBase cmd = GetSelectCommand(canvas);

            if (cmd == null)
                return;

            bool isAdd = (Keyboard.Modifiers & ModifierKeys.Control) != 0;
            if (canvas.DataContext is TileGridViewModel vm)
            {
                IsDragging = false;

                Vec2<int>? hoveredTile = GetHoveredTile(canvas, e.GetPosition(canvas));
                if (hoveredTile != null)
                    cmd.Execute(new TileSelectionArgs(hoveredTile, isAdd, DragStage.End));
            }
        }
    }
}
