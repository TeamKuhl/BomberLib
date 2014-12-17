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
    public delegate void ComMessageHandler(TcpClient client, String message);

    // connection events
    public delegate void ComConnectionHandler(TcpClient client);

    // unknown type event
    public delegate void ComUnknownTypeHandler(TcpClient client, String type, String message);


    public class Communication
    {
        // events
        public ComConnectionHandler onDisconnect;
        public ComConnectionHandler onConnect;
        public ComMessageHandler onPlayerInfo;
        public ComMessageHandler onMove;
        public ComMessageHandler onChatMessage;
        public ComMessageHandler onCommand;
        public ComMessageHandler onBombPlace;
        public ComMessageHandler onItemUse;
        public ComMessageHandler onSetPlayerStatus;
        public ComMessageHandler onGetMap;
        public ComMessageHandler onGetPlayerPosition;
        public ComMessageHandler onGetPlayerList;
        public ComMessageHandler onGetPlayerStatus;
        public ComMessageHandler onGetRoundStatus;
        public ComMessageHandler onGetPlayerModel;
        public ComMessageHandler onGetModelList;
        public ComMessageHandler onSetPlayerModel;
        public ComMessageHandler onGetPlayerScore;
        public ComMessageHandler onGetTextures;

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
            server.onConnect += new ServerConnectHandler(ConnectHandler);
        }

        /// <summary>
        /// Handler to call events on messages
        /// </summary>
        /// <param name="client">client the message comes from</param>
        /// <param name="type">type</param>
        /// <param name="message">message</param>
        private void ReceiveMessageHandler(TcpClient client, String type, String message)
        {
            // switch types & call events
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
                case "GetMap":
                    if (onGetMap != null) onGetMap(client, message);
                    break;
                case "GetPlayerPosition":
                    if (onGetPlayerPosition != null) onGetPlayerPosition(client, message);
                    break;
                case "GetPlayerList":
                    if (onGetPlayerList != null) onGetPlayerList(client, message);
                    break;
                case "GetPlayerStatus":
                    if (onGetPlayerStatus != null) onGetPlayerStatus(client, message);
                    break;
                case "GetRoundStatus":
                    if (onGetRoundStatus != null) onGetRoundStatus(client, message);
                    break;
                case "SetPlayerStatus":
                    if (onSetPlayerStatus != null) onSetPlayerStatus(client, message);
                    break;
                case "GetPlayerModel":
                    if (onGetPlayerModel != null) onGetPlayerModel(client, message);
                    break;
                case "GetModelList":
                    if (onGetModelList != null) onGetModelList(client, message);
                    break;
                case "SetPlayerModel":
                    if (onSetPlayerModel != null) onSetPlayerModel(client, message);
                    break;
                case "GetPlayerScore":
                    if (onGetPlayerScore != null) onGetPlayerScore(client, message);
                    break;
                case "GetTextures":
                    if (onGetTextures != null) onGetTextures(client, message);
                    break;
                default:
                    if (onUnknownType != null) onUnknownType(client, type, message);
                    break;
            }
        }

        /// <summary>
        ///     Handler for player disconnects 
        /// </summary>
        /// <param name="client"></param>
        public void DisconnectHandler(TcpClient client)
        {
            if (onDisconnect != null) onDisconnect(client);
        }

        /// <summary>
        ///     Handler for player connects 
        /// </summary>
        /// <param name="client"></param>
        public void ConnectHandler(TcpClient client)
        {
            if (onConnect != null) onConnect(client);
        }

        /// <summary>
        ///     send message to all
        /// </summary>
        /// <param name="type"></param>
        /// <param name="message"></param>
        public void sendToAll(string type, string message)
        {
            try
            {
                // use communication library 
                server.sendToAll(type, message);
            }
            catch (Exception e) { e.GetType(); }

        }

        /// <summary>
        ///     send message to a client
        /// </summary>
        /// <param name="client"></param>
        /// <param name="type"></param>
        /// <param name="message"></param>
        public void send(TcpClient client, string type, string message)
        {
            // use communication library 
            server.send(client, type, message);
        }

        /// <summary>
        ///     send message to all clients except one
        /// </summary>
        /// <param name="client"></param>
        /// <param name="type"></param>
        /// <param name="message"></param>
        public void sendToAllExcept(TcpClient client, string type, string message)
        {
            // use communication library 
            server.sendToAllExcept(client, type, message);
        }
    }
}
