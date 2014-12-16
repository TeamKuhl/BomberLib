﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Diagnostics;

namespace BomberLib
{

    // dead event for game class
    public delegate void PlayerDiedHandler();
    public delegate void PlayerKillHandler(int playerID);

    public class Player
    {
        // declaration
        public TcpClient client;
        private Communication com;

        // events
        public PlayerDiedHandler onPlayerDied;
        public PlayerChangeHandler onPlayerChange;
        public PlayerKillHandler onPlayerKill;

        // playername
        public string name = "";

        // tcpsocketid
        public int socketID = 0;

        // imagestring
        public string image;

        // score
        public int score = 0;
        public int kills = 0;
        public int deaths = 0;

        // position
        public int X, Y;

        // current settings
        public int size = 3;
        public int maxamount = 3;
        public int curamount = 0;
        public int time = 3;
        public int speed = 100;

        // statuscode
        // [1: playing, 2: waiting, 3: spectating]
        public int status = 0;

        // stopwatch for speed
        private Stopwatch SpeedLimit;
        
        /// <summary>
        ///     Create new player object
        /// </summary>
        /// <param name="PlayerName"></param>
        /// <param name="PlayerConnection"></param>
        /// <param name="communication"></param>
        public Player(String PlayerName, TcpClient PlayerConnection, Communication communication)
        {
            this.com = communication;
            this.name = PlayerName;
            this.client = PlayerConnection;
            this.socketID = this.client.Client.GetHashCode();
        }

        /// <summary>
        ///     change position
        /// </summary>
        /// <param name="posx"></param>
        /// <param name="posy"></param>
        /// <returns></returns>
        public Boolean setPosition(int posx, int posy)
        {
            // speed limit
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

        /// <summary>
        /// set the status of a player
        /// </summary>
        /// <param name="statuscode"></param>
        /// <returns></returns>
        public Boolean setStatus(int statuscode)
        {
            // check for valid status
            if (statuscode >= 1 && statuscode <= 2)
            {
                // set status
                this.status = statuscode;
                
                // tell the world what changed
                this.com.sendToAll("PlayerStatus", this.socketID + ":" + this.status);

                // player change
                if (this.onPlayerChange != null) this.onPlayerChange();

                return true;
            }
            else return false;
        }

        /// <summary>
        ///     player died
        /// </summary>
        public void die(int playerID)
        {
            if (this.status == 1)
            {
                // set position
                this.setStatus(2);

                // moa deaaad
                this.deaths++;

                // no kill points for suiciders
                if (playerID != this.socketID)
                {
                    if(this.onPlayerKill != null) onPlayerKill(playerID);

                }
                else this.score--;

                // update score
                this.sendScoreToAll();

                // send this
                this.com.sendToAll("PlayerDied", this.socketID + ":" + playerID);

                // call event
                if (this.onPlayerDied != null) this.onPlayerDied();
            }
        }

        /// <summary>
        /// send score to all on change
        /// </summary>
        public void sendScoreToAll()
        {
            string scoreMessage = this.socketID + ":" + this.score + ":" + this.kills + ":" + this.deaths;
            this.com.sendToAll("PlayerScore", scoreMessage);
        }

        public void sendScoreTo(TcpClient client)
        {
            string scoreMessage = this.socketID + ":" + this.score + ":" + this.kills + ":" + this.deaths;
            this.com.send(client, "PlayerScore", scoreMessage);
        }





    }
}
