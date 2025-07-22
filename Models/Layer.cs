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
       
        public Layer(string name)
        {
            this.Name = name;
            Opacity = 1.0;
            Visible = true;
        }

    }
}
