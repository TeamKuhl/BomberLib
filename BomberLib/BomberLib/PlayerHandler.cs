using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Collections;

namespace BomberLib
{
    public class PlayerHandler
    {
        private Communication com;
        public Dictionary<int, Player> players = new Dictionary<int, Player>();

        public PlayerHandler(Communication c)
        {
            this.com = c;

            // events
            this.com.onPlayerInfo           += new ComMessageHandler(JoinHandler);
            this.com.onDisconnect           += new ComConnectionHandler(LeaveHandler);
            this.com.onGetPlayerPosition    += new ComMessageHandler(GetPlayerPositionHandler);
            this.com.onGetPlayerList        += new ComMessageHandler(GetPlayerListHandler);
            
        }

        /// <summary>
        ///     Handler for join events
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message"></param>
        public void JoinHandler(TcpClient client, String message)
        {
            this.players[client.Client.GetHashCode()] = new Player(message, client, com);
            this.players[client.Client.GetHashCode()].setPosition(2, 2);
            this.com.sendToAll("Join", Convert.ToString(client.Client.GetHashCode())+":"+message);
            Console.WriteLine("Player "+message+" connected [#"+client.Client.GetHashCode()+"]");
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
                players.Remove(SocketID);
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

        /// <summary>
        /// get the player dictionary
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, Player> getAllPlayers()
        {
            return players;
        }
    }
}
