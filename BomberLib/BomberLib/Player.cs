using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace BomberLib
{
    public class Player
    {
        public TcpClient client;
        private Communication com;
        public string name = "";
        public int socketID = 0;
        public int X, Y;
        /*public string imgHash = "";
        public double locationX = 0;
        public double locationY = 0;
        public int maxBombs = 1;
        public int bombsActive = 1;
        public int bombSize = 1;
        public double speed = 1.0;*/

        public Player(String PlayerName, TcpClient PlayerConnection, Communication communication)
        {
            this.com = communication;
            this.name = PlayerName;
            this.client = PlayerConnection;
            this.socketID = this.client.Client.GetHashCode();
        }

        public void setPosition(int posx, int posy)
        {
            this.X = posx;
            this.Y = posy;
            this.com.sendToAll("PlayerPosition", this.socketID + ":" + this.X + ":" + this.Y);
        }






    }
}
