using NotesApp.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TileBasedLevelEditor.Services;

namespace TileBasedLevelEditor.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ICustomNavigationService _navigationService;
        public TilesetViewModel TilesetViewModel { get; private set; }
        public MainWindowViewModel(ICustomNavigationService navigationService) 
        { 
            _navigationService = navigationService;
            TilesetViewModel = new TilesetViewModel(_navigationService);
        }
    }
}
