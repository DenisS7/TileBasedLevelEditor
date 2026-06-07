using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Navigation;
using TileBasedLevelEditor.Services;
using TileBasedLevelEditor.ViewModels;
using TileBasedLevelEditor.Views;

namespace TileBasedLevelEditor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            CustomNavigationService navigationService = new CustomNavigationService();
            TilesetsService tilesetsService = new TilesetsService();
            TilemapRendererService tilemapRendererService = new TilemapRendererService(tilesetsService);
            MainWindow = new MainWindow()
            {
                DataContext = new MainWindowViewModel(navigationService, tilesetsService, tilemapRendererService)
            };
            MainWindow.Show();
            base.OnStartup(e);
        }
    }

}
