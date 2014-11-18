﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Collections;
using System.Threading;

namespace BomberLib
{
    // bomberlib events
    public delegate void PlayerChangedPositionHandler(String PlayerName, int X, int Y);
    public delegate void PlayerChangeHandler();

    public class PlayerHandler
    {
        private Communication com;
        public Dictionary<int, Player> players = new Dictionary<int, Player>();

        // events
        public PlayerDiedHandler onPlayerDied;
        public PlayerChangeHandler onPlayerChange;

        public PlayerHandler(Communication c)
        {
            this.com = c;

            // events
            this.com.onPlayerInfo           += new ComMessageHandler(JoinHandler);
            this.com.onDisconnect           += new ComConnectionHandler(LeaveHandler);
            this.com.onConnect              += new ComConnectionHandler(ConnectHandler);
            this.com.onGetPlayerPosition    += new ComMessageHandler(GetPlayerPositionHandler);
            this.com.onGetPlayerList        += new ComMessageHandler(GetPlayerListHandler);
            this.com.onGetPlayerStatus      += new ComMessageHandler(GetPlayerStatusHandler);
            this.com.onSetPlayerStatus      += new ComMessageHandler(SetPlayerStatusHandler);
            
        }

        /// <summary>
        ///     Handler for join events
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message"></param>
        public void JoinHandler(TcpClient client, String message)
        {
            // output
            Console.WriteLine("Player " + message + " connected [#" + client.Client.GetHashCode() + "]");
            this.com.sendToAll("Join", Convert.ToString(client.Client.GetHashCode()) + ":" + message);

            // create player
            this.players[client.Client.GetHashCode()] = new Player(message, client, com);
            this.players[client.Client.GetHashCode()].setStatus(2);

            // listen to dead
            this.players[client.Client.GetHashCode()].onPlayerDied += new PlayerDiedHandler(PlayerDiedHandler);

            // listen to change
            this.players[client.Client.GetHashCode()].onPlayerChange += new PlayerChangeHandler(PlayerChangeHandler);

            // player change
            if (this.onPlayerChange != null) this.onPlayerChange();
        }

        /// <summary>
        ///  Handler for leave events
        /// </summary>
        /// <param name="client"></param>
        public void LeaveHandler(TcpClient client)
        {
            int SocketID = client.Client.GetHashCode();

            if (players.ContainsKey(SocketID))
            {
                Console.WriteLine("Player " + players[SocketID].name + " disconnected [#" + SocketID + "]");
                this.com.sendToAllExcept(client, "Leave", Convert.ToString(SocketID));

                // player change
                if (this.onPlayerChange != null) this.onPlayerChange();

                // remove
                players.Remove(SocketID);

                // died
                if (this.onPlayerDied != null) this.onPlayerDied();

            }
        }

        /// <summary>
        /// Handles getplayerposition requests
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message"></param>
        public void GetPlayerPositionHandler(TcpClient client, String message)
        {
            if(players.ContainsKey(Convert.ToInt32(message)))
            {
                // get player
                Player p = players[Convert.ToInt32(message)];

                // send player position
                this.com.send(client, "PlayerPosition", p.socketID + ":" + p.X + ":" + p.Y);
            }
        }

        /// <summary>
        /// Handles GetPlayerStatus requests
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message"></param>
        public void GetPlayerStatusHandler(TcpClient client, String message)
        {
            // convert message to int
            int socketID = Convert.ToInt32(message);

            // check if player is on this server
            if(this.players.ContainsKey(socketID))
            {
                // send answer
                this.com.send(client, "PlayerStatus", socketID + ":" + this.players[socketID].status);
            }
        }

        /// <summary>
        /// Handles SetPlayerStatus requests
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message"></param>
        public void SetPlayerStatusHandler(TcpClient client, String message)
        {
            // parse message
            int statuscode = Convert.ToInt32(message);

            // set status
            this.players[client.Client.GetHashCode()].setStatus(statuscode);

            // player change
            if (this.onPlayerChange != null) this.onPlayerChange();

        }

        /// <summary>
        /// Handles GetPlayerList requests
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message"></param>
        public void GetPlayerListHandler(TcpClient client, String message)
        {
            // final string
            String playerlist = "";

            foreach(KeyValuePair<int, Player> p in players)
            {
                playerlist += p.Key + ":" + p.Value.name + ";";
            }

            // send back to client
            if(playerlist != "")
            {
                this.com.send(client, "PlayerList", playerlist);
            }
        }

        /// <summary>
        ///     get a player by his hashcode
        /// </summary>
        /// <param name="hashCode"></param>
        /// <returns></returns>
        public Player getPlayerByHashcode(int hashCode)
        {
            return players[hashCode];
        }

        /// <summary>
        ///     get the amount of current players
        /// </summary>
        /// <returns></returns>
        public int getTotalPlayerCount()
        {
            return players.Count;
        }

        public int getWaitingPlayerCount()
        {
            int waitingPlayer = 0;

            // loop all players
            foreach (KeyValuePair<int, Player> p in this.players)
            {
                // check if player is waiting
                if (p.Value.status <= 2)
                {
                    waitingPlayer++;
                }
            }

            // return
            return waitingPlayer;

        }

        public int getLivingPlayerCount()
        {
            int livingPlayer = 0;

            foreach (KeyValuePair<int, Player> p in this.players)
            {
                if (p.Value.status == 1)
                {
                    livingPlayer++;
                }
            }

            return livingPlayer;
        }

        /// <summary>
        /// get the player dictionary
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, Player> getAllPlayers()
        {
            return players;
        }

        /// <summary>
        /// Check if there is a player on this position
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <returns></returns>
        public Boolean isPlayerOnPosition(int X, int Y)
        {
            // return boolean
            Boolean isPlayer = false;

            // loop all players
            foreach(KeyValuePair<int, Player> p in this.players)
            {
                // check if player is on position
                if(p.Value.X == X && p.Value.Y == Y)
                {
                    // there IS a player
                    isPlayer = true;
                }
            }

            // return boolean
            return isPlayer;
        }

        /// <summary>
        /// get the player on a position, USE "isPlayerOnPosition" BEFORE YOU USE THIS
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <returns></returns>
        public Player getPlayerOnPosition(int X, int Y)
        {
            // loop all players
            foreach (KeyValuePair<int, Player> p in this.players)
            {
                // check if player is on position
                if (p.Value.X == X && p.Value.Y == Y)
                {
                    return p.Value;
                }
            }
            return null;
        }

        public Player getLastLivingPlayer()
        {
            foreach (KeyValuePair<int, Player> p in this.players)
            {
                if (p.Value.status == 1)
                {
                    return p.Value;
                }
            }
            return null;
        }


        public void PlayerDiedHandler()
        {
            // pass event
            if (this.onPlayerDied != null) this.onPlayerDied();

            // player change
            if (this.onPlayerChange != null) this.onPlayerChange();
        }


        public void PlayerChangeHandler()
        {
            // player change
            if (this.onPlayerChange != null) this.onPlayerChange();
        }

        public void ConnectHandler(TcpClient client)
        {
            this.com.send(client, "YourId", client.Client.GetHashCode().ToString());
        }
    }
}
