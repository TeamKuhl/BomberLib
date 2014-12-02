using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace BomberLib
{

    // bomb explosion event
    public delegate void BombExplosionHandler(int X, int Y, int size, int playerID);

    class Bomb
    {
        // event
        public BombExplosionHandler onBombExplosion;

        //position
        public int X, Y;

        // size
        public int size;

        // time
        public int time;

        // by player
        public int playerID;

        // initialize
        public Bomb(int BombSize, int BombTime, int PosX, int PosY, int Placer)
        {
            // set variables
            this.X = PosX;
            this.Y = PosY;
            this.size = BombSize;
            this.time = BombTime;
            this.playerID = Placer;

            // start timer in a thread
            Thread thread = new Thread(this.wait);
            thread.Start();
        }

        /// <summary>
        /// waits the bomb time and calls the event
        /// </summary>
        private void wait()
        {
            Thread.Sleep(this.time * 1000);
            if(this.onBombExplosion != null)
            {
                this.onBombExplosion(this.X, this.Y, this.size, this.playerID);
            }
        }


    }
}
