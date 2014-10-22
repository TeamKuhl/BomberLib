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
        private Dictionary<int, Player> players = new Dictionary<int, Player>();

        public PlayerHandler(Communication c)
        {
            this.com = c;

            // events
            this.com.onPlayerInfo += new ComMessageHandler(JoinHandler);
            this.com.onDisconnect += new ComConnectionHandler(LeaveHandler);
            
        }

        /// <summary>
        ///     Handler for join events
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message"></param>
        public void JoinHandler(TcpClient client, String message)
        {
            this.players[client.Client.GetHashCode()] = new Player(message, client);
            this.com.sendToAll("Join", message);
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
                this.com.sendToAllExcept(client, "Leave", players[SocketID].name);
                players.Remove(SocketID);
            }
        }

        public Player getPlayerByHashcode(int hashCode)
        {
            return players[hashCode];
        }

    }
}
