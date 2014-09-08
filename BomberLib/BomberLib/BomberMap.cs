using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BomberLib
{
    class BomberMap
    {
        public int height = 0;
        public int width = 0;
        public Dictionary<int, Dictionary<int, MapTile>> tiles;
    }
}
