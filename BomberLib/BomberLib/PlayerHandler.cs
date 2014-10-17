﻿using System;
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

        public void JoinHandler(TcpClient client, String message)
        {
            this.players[client.Client.GetHashCode()] = new Player(message, client);
            this.com.sendToAll("join", message);
            Console.WriteLine("Player "+message+" connected [#"+client.Client.GetHashCode()+"]");
        }

        public void LeaveHandler(TcpClient client)
        {
            int SocketID = client.Client.GetHashCode();

            if (players[SocketID] != null)
            {
                Console.WriteLine("Player " + players[SocketID].name + " disconnected [#" + SocketID + "]");
                this.com.sendToAllExcept(client, "leave", players[SocketID].name);
                players.Remove(SocketID);
            }
        }

    }
}
