using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NotesApp.Commands;
using TileBasedLevelEditor.Models;
using TileBasedLevelEditor.ViewModels;

namespace TileBasedLevelEditor.Misc
{
    public static class TileSelector
    {
        public static readonly DependencyProperty HoverCommandProperty =
            DependencyProperty.RegisterAttached(
                "HoverCommand",
                typeof(CommandBase),
                typeof(TileSelector),
                new PropertyMetadata(null, OnHoverCommandChanged)
                );

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

            if (!(canvas.DataContext is TilesetViewModel vm) || vm.CurrentTileset == null)
            {
                cmd.Execute(null);
                return;
            }

            Point p = e.GetPosition(canvas);

            Vec2<int> tileSize = vm.CurrentTileset.TileSize;
            Vec2<int> gridSize = vm.ImageSize / tileSize;

            if (tileSize.X <= 0 || tileSize.Y <= 0)
            {
                cmd.Execute(null);
                return;
            }

            Vec2<int> HoveredTileIndex = new Vec2<int>((int)(p.X / tileSize.X), (int)(p.Y / tileSize.Y));
            if (HoveredTileIndex.X < 0 || HoveredTileIndex.X >= gridSize.X || HoveredTileIndex.Y < 0 ||
                HoveredTileIndex.Y >= gridSize.Y)
            {
                cmd.Execute(null);
            }
            else
            {
                cmd.Execute(HoveredTileIndex);
            }
        }

        private static void Canvas_MouseLeave(object sender, MouseEventArgs e)
        {
            Canvas canvas = (Canvas)sender;
            CommandBase cmd = GetHoverCommand(canvas);
            cmd?.Execute(null);
        }
    }
}
