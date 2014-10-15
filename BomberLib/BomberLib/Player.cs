using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BomberLib
{
    class Player
    {
        public string name = "";
        public int socketID = 0;
        /*public string imgHash = "";
        public double locationX = 0;
        public double locationY = 0;
        public int maxBombs = 1;
        public int bombsActive = 1;
        public int bombSize = 1;
        public double speed = 1.0;*/

        public Player(int socketHashCode)
        {
            this.socketID = socketHashCode;
        }




    }
}
