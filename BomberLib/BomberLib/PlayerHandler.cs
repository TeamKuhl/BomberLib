using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Collections;
using System.Threading;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace BomberLib
{
    // bomberlib events
    public delegate void PlayerChangedPositionHandler(String PlayerName, int X, int Y);
    public delegate void PlayerChangeHandler();

    public class PlayerHandler
    {
        private Communication com;
        public Dictionary<int, Player> players = new Dictionary<int, Player>();
        private Dictionary<string, string> models = new Dictionary<string, string>();

        // events
        public PlayerDiedHandler onPlayerDied;
        public PlayerChangeHandler onPlayerChange;

        public PlayerHandler(Communication c)
        {
            this.com = c;

            this.loadPlayerModels();

            // events
            this.com.onPlayerInfo           += new ComMessageHandler(JoinHandler);
            this.com.onDisconnect           += new ComConnectionHandler(LeaveHandler);
            this.com.onConnect              += new ComConnectionHandler(ConnectHandler);
            this.com.onGetPlayerPosition    += new ComMessageHandler(GetPlayerPositionHandler);
            this.com.onGetPlayerList        += new ComMessageHandler(GetPlayerListHandler);
            this.com.onGetPlayerStatus      += new ComMessageHandler(GetPlayerStatusHandler);
            this.com.onSetPlayerStatus      += new ComMessageHandler(SetPlayerStatusHandler);
            this.com.onGetPlayerModel       += new ComMessageHandler(GetPlayerModelHandler);
            this.com.onGetModelList         += new ComMessageHandler(GetModelListHandler);
            this.com.onSetPlayerModel       += new ComMessageHandler(SetPlayerModelHandler);
            this.com.onGetPlayerScore       += new ComMessageHandler(GetPlayerScoreHandler);
            
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

            // create player
            this.players[client.Client.GetHashCode()] = new Player(message, client, com);
            this.players[client.Client.GetHashCode()].setStatus(2);

            // default cow
            this.players[client.Client.GetHashCode()].image = models["Cow"];


            this.com.sendToAll("Join", Convert.ToString(client.Client.GetHashCode()) + ":" + message);

            // listen to dead
            this.players[client.Client.GetHashCode()].onPlayerDied += new PlayerDiedHandler(PlayerDiedHandler);

            // listen to kills
            this.players[client.Client.GetHashCode()].onPlayerKill += new PlayerKillHandler(PlayerKillHandler);

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
        /// Hanlde Player score requests
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message"></param>
        public void GetPlayerScoreHandler(TcpClient client, String message)
        {
            int playerID = Convert.ToInt32(message);

            this.players[playerID].sendScoreTo(client);
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
        /// Handles player kills for score
        /// </summary>
        /// <param name="killerID"></param>
        public void PlayerKillHandler(int killerID)
        {
            this.players[killerID].kills++;
            this.players[killerID].score++;
            this.players[killerID].sendScoreToAll();
        }

        /// <summary>
        /// Handles player model requests
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message"></param>
        public void GetPlayerModelHandler(TcpClient client, string message)
        {
            this.com.send(client, "PlayerModel", message+":"+players[Convert.ToInt32(message)].image);
        }

        /// <summary>
        /// Handles model list requests
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message"></param>
        public void GetModelListHandler(TcpClient client, string message)
        {
            this.com.send(client, "ModelList", getModelList());
        }

        /// <summary>
        /// Handles player model sets
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message"></param>
        public void SetPlayerModelHandler(TcpClient client, string message)
        {
            int id = client.Client.GetHashCode();

            if (models.ContainsKey(message))
            {
                this.players[id].switchModel(models[message]);
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

        /// <summary>
        /// get the last living player on the field to select the winner
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Handle "PlayerDied" events from player class to pass to Game
        /// </summary>
        public void PlayerDiedHandler()
        {
            // pass event
            if (this.onPlayerDied != null) this.onPlayerDied();

            // player change
            if (this.onPlayerChange != null) this.onPlayerChange();
        }

        /// <summary>
        /// Handles player change events to try to search for a winner
        /// </summary>
        public void PlayerChangeHandler()
        {
            // player change
            if (this.onPlayerChange != null) this.onPlayerChange();
        }

        /// <summary>
        /// Handle connect event to send the id back to the client
        /// </summary>
        /// <param name="client"></param>
        public void ConnectHandler(TcpClient client)
        {
            this.com.send(client, "YourId", client.Client.GetHashCode().ToString());
        }

        /// <summary>
        /// loads the player models from file
        /// </summary>
        public void loadPlayerModels()
        {
            // define path
            string modelPath = "models";

            // get files in models directory
            string[] files = Directory.GetFiles(modelPath);

            // clear dictionary
            models.Clear();

            // loop files
            foreach (string file in files)
            {
                // load image
                Image img = Image.FromFile(file);

                String[] splitted = file.Split(Path.DirectorySeparatorChar);

                string rawFileName = splitted[splitted.Length -1];

                // image name
                string imageName = rawFileName.Substring(0, rawFileName.Length-4);

                // convert to string
                MemoryStream memory = new MemoryStream();
                img.Save(memory, ImageFormat.Png);
                string imageEncoded = Convert.ToBase64String(memory.ToArray());
                memory.Close();

                // add to dictionary
                models.Add(imageName, imageEncoded);
            }
        }


        /// <summary>
        /// parses model list to a string
        /// </summary>
        /// <returns></returns>
        private string getModelList()
        {
            string modelList = "";

            foreach (KeyValuePair<string, string> model in models)
            {
                modelList += model.Key+":"+model.Value+";";
            }

            return modelList;
        }
    }
}
