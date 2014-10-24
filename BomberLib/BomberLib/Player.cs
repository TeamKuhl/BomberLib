using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Diagnostics;

namespace BomberLib
{

    public class Player
    {
        // declaration
        public TcpClient client;
        private Communication com;
        public string name = "";
        public int socketID = 0;
        public int X, Y;
        public int size = 3;
        public int time = 3;
        public int speed = 100;

        // stopwatch
        private Stopwatch SpeedLimit;
        

        public Player(String PlayerName, TcpClient PlayerConnection, Communication communication)
        {
            if (PlayerName == "Matthias") this.speed = 0;

            this.com = communication;
            this.name = PlayerName;
            this.client = PlayerConnection;
            this.socketID = this.client.Client.GetHashCode();
        }

        public Boolean setPosition(int posx, int posy)
        {
            if (SpeedLimit == null || SpeedLimit.ElapsedMilliseconds > speed)
            {
                if (SpeedLimit != null) SpeedLimit.Stop();
                // set new player position
                this.X = posx;
                this.Y = posy;

                // restart stopwatch
                SpeedLimit = Stopwatch.StartNew();

                this.com.sendToAll("PlayerPosition", this.socketID + ":" + this.X + ":" + this.Y);

                return true;
            }
            else return false;
        }

        public void die()
        {

            this.X = 2;
            this.Y = 2;

            this.com.sendToAll("PlayerDied", this.socketID + "");
            this.com.sendToAll("PlayerPosition", this.socketID + ":" + this.X + ":" + this.Y);
        }






    }
}
