using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TileBasedLevelEditor.ViewModels;
using TileBasedLevelEditor.Views;

namespace TileBasedLevelEditor.Services
{
    public class CustomNavigationService : ICustomNavigationService
    {
        public CustomNavigationService() { }
        public void OpenNewTilesetDialog(TilesetViewModel tilesetViewModel)
        {
            NewTilesetDialogView newTilesetDialogView = new NewTilesetDialogView()
            {
                DataContext = tilesetViewModel
            };
           
            tilesetViewModel.RequestCloseNewTilesetDialog += () =>
            {
                newTilesetDialogView.Close();
            };
            newTilesetDialogView.Show();
        }
    }
}
