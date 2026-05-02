using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TileBasedLevelEditor.Models
{
    public class Layer
    {
        public string Name { get; set; }
        public double Opacity { get; set; }
        public bool Visible { get; set; }
        public TileData[] Tiles { get; set; }
        public int VisibilityIndex { get; set; }

        public Layer(string name, int size, int visibilityIndex)
        {
            this.Name = name;
            Tiles = new TileData[size];
            VisibilityIndex = visibilityIndex;

            Opacity = 1.0;
            Visible = true;
        }

    }
}
