using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace BomberLib
{
    class Chat
    {

        // declaration
        private Communication communication;
        private PlayerHandler players;

        public Chat(Communication com, PlayerHandler pl)
        {
            this.communication = com;
            this.players = pl;

            // listen to event
            this.communication.onChatMessage += new ComMessageHandler(ChatMessageHandler);
        }

        /// <summary>
        ///     Chat message handler
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message"></param>
        public void ChatMessageHandler(TcpClient client, String message)
        {
            // get player
            Player p = players.getPlayerByHashcode(client.Client.GetHashCode());

            // console
            Console.WriteLine(">> "+p.name+": "+message);

            // send message to all clients
            this.communication.sendToAll("ChatMessage", p.name+": "+message);
        }

    }
}
