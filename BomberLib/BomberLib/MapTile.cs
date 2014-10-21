using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BomberLib
{
    class MapTile
    {
        public int type = 0;
        public int x, y;

        public MapTile(int fieldType, int row, int tile)
        {
            // set vars
            this.type = fieldType;
            this.x = tile;
            this.y = row;
        }
    }
}
