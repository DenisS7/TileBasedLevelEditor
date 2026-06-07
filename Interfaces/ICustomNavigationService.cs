using TileBasedLevelEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TileBasedLevelEditor.Services
{
    public interface ICustomNavigationService
    {
        public void OpenNewTilesetDialog(TilesetViewModel tilesetViewModel);
        public void OpenNewTilemapDialog(TilemapEditorViewModel tilemapEditorViewModel);
        public void OpenEditTilemapDialog(TilemapEditorViewModel tilemapEditorViewModel);
    }
}
