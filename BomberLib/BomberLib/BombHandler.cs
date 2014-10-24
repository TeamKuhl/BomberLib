using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace BomberLib
{
    class BombHandler
    {
        // bomb list
        public List<Bomb> bombs = new List<Bomb>();

        // commclass
        private Communication com;

        // bomb explosion event
        public BombExplosionHandler onBombExplosion;

        public BombHandler(Communication communication)
        {
            this.com = communication;
        }

        /// <summary>
        ///     place a bomb at a location with a size and a time until it explodes
        /// </summary>
        /// <param name="size"></param>
        /// <param name="time"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        public void placeBomb(int size, int time, int X, int Y)
        {
            // create bomb
            Bomb bomb = new Bomb(size, time, X, Y);

            // listen to event
            bomb.onBombExplosion += new BombExplosionHandler(BombExplosionHandler);

            // add to bomb list
            this.bombs.Add(bomb);

            // share
            this.com.sendToAll("BombPlaced", X + ":" + Y);
        }

        /// <summary>
        ///     Handles bomb explosions
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="size"></param>
        public void BombExplosionHandler(int X, int Y, int size)
        {
            // loop bombs to remove the one which exploded
            foreach(Bomb bomb in this.bombs)
            {
                if(bomb.X == X && bomb.Y == Y)
                {
                    this.bombs.Remove(bomb);
                    break;
                }
            }

            // call event for further actions
            if(this.onBombExplosion != null) this.onBombExplosion(X, Y, size);
        }

        public Boolean isBombAtPosition(int X, int Y)
        {
            // loop bombs 
            foreach (Bomb bomb in this.bombs)
            {
                if (bomb.X == X && bomb.Y == Y)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
