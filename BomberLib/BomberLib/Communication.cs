using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunicationLibrary;
using System.Net.Sockets;

namespace BomberLib
{
    // message events
    public delegate void ComMessageHandler         (TcpClient client, String message);

    // unknown type event
    public delegate void ComUnknownTypeHandler     (TcpClient client, String type, String message);


    class Communication
    {
        public ComMessageHandler        onPlayerInfo;

        public ComMessageHandler              onMove;
        public ComMessageHandler       onChatMessage;
        public ComMessageHandler           onCommand;
        public ComMessageHandler         onBombPlace;
        public ComMessageHandler           onItemUse;

        // else
        public ComUnknownTypeHandler    onUnknownType;

        // declaration
        private Server server;

        /// <summary>
        /// initialize communication object with server object of a running server
        /// </summary>
        /// <param name="s">server object of a running server</param>
        public Communication(Server s)
        {
            this.server = s;

            // listen to server events
            server.onReceive    +=  new ServerReceiveHandler(ReceiveMessageHandler);
        }

        /// <summary>
        /// Handler to call events on messages
        /// </summary>
        /// <param name="client">client the message comes from</param>
        /// <param name="type">type</param>
        /// <param name="message">message</param>
        private void ReceiveMessageHandler(TcpClient client, String type, String message)
        {
            // switch types
            switch(type)
            {
                case "PlayerInfo":
                    onPlayerInfo(client, message);
                    break;
                case "ChatMessage":
                    onChatMessage(client, message);
                    break;
                case "Command":
                    onCommand(client, message);
                    break;
                case "BombPlace":
                    onBombPlace(client, message);
                    break;
                case "ItemUse":
                    onItemUse(client, message);
                    break;
                case "Move":
                    onMove(client, message);
                    break;
                default:
                    onUnknownType(client, type, message);
                    break;
            }
        }


    }
}
