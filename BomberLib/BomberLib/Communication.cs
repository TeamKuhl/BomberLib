﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunicationLibrary;
using System.Net.Sockets;

namespace BomberLib
{
    // message events
    public delegate void ComMessageHandler(TcpClient client, String message);

    // connection events
    public delegate void ComConnectionHandler(TcpClient client);

    // unknown type event
    public delegate void ComUnknownTypeHandler(TcpClient client, String type, String message);


    public class Communication
    {
        public ComConnectionHandler onDisconnect;

        public ComMessageHandler onPlayerInfo;

        public ComMessageHandler onMove;
        public ComMessageHandler onChatMessage;
        public ComMessageHandler onCommand;
        public ComMessageHandler onBombPlace;
        public ComMessageHandler onItemUse;

        // else
        public ComUnknownTypeHandler onUnknownType;

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
            server.onReceive += new ServerReceiveHandler(ReceiveMessageHandler);
            server.onDisconnect += new ServerDisconnectHandler(DisconnectHandler);
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
            switch (type)
            {
                case "PlayerInfo":
                    if (onPlayerInfo != null) onPlayerInfo(client, message);
                    break;
                case "ChatMessage":
                    if (onChatMessage != null) onChatMessage(client, message);
                    break;
                case "Command":
                    if (onCommand != null) onCommand(client, message);
                    break;
                case "BombPlace":
                    if (onBombPlace != null) onBombPlace(client, message);
                    break;
                case "ItemUse":
                    if (onItemUse != null) onItemUse(client, message);
                    break;
                case "Move":
                    if (onMove != null) onMove(client, message);
                    break;
                default:
                    if (onUnknownType != null) onUnknownType(client, type, message);
                    break;
            }
        }

        public void DisconnectHandler(TcpClient client)
        {
            if (onDisconnect != null) onDisconnect(client);
        }

        public void sendToAll(string type, string message)
        {
            server.sendToAll(type, message);
        }

        public void send(TcpClient client, string type, string message)
        {
            server.send(client, type, message);
        }

        public void sendToAllExcept(TcpClient client, string type, string message)
        {
            server.sendToAllExcept(client, type, message);
        }
    }
}
