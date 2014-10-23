using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using CommunicationLibrary;

namespace BomberLib
{
    public class Game
    {
        // declaration
        Config config;
        BomberMap bomberMap;
        Server server;
        Communication com;
        PlayerHandler ph;
        Chat chat;

        /// <summary>
        ///     Starts the server
        /// </summary>
        /// <returns>Success state of game start, mainly communication.</returns>
        public Boolean start()
        {
            // load config
            config = new Config();

            // create server
            server = new Server();
            
            // start server
            server.start(45454); // TODO Port from Config

            // output
            Console.WriteLine("Server started, listening on port " + 45454);

            // create communication object
            com = new Communication(server);

            // load map
            bomberMap = new BomberMap("bomberMap", com); // TODO Map name from config

            // create playerhandler
             ph = new PlayerHandler(com);

            // start chat 
            chat = new Chat(com, ph);

            // start new round
            this.tryNewRound();
                        
            return true;
        }
        /// <summary>
        ///     waits for a new round to start and then starts it
        /// </summary>
        public void tryNewRound()
        {
            // log
            Console.WriteLine("Waiting for enough players to start a new round.");

            // wait for new round
            while (this.ph.getTotalPlayerCount() < 1) { } // TODO Minplayers from config
            
            // start round
            this.newRound();
        }

        /// <summary>
        /// starts a new round
        /// </summary>
        /// <returns></returns>
        public Boolean newRound()
        {
            // output
            Console.WriteLine("New round started. Spawning players.");

            // how many players?
            int playerCount = this.ph.getTotalPlayerCount();

            // tell all what happens
            this.com.sendToAll("NewRoundStart", Convert.ToString(playerCount));

            // get spawn positions for players
            List<string> spawnPositions = this.bomberMap.getSpawnPositions(playerCount);

            // get all players
            Dictionary<int, Player> allPlayers = this.ph.getAllPlayers();

            // counter for IENumerable 
            int counter = 0;

            // loop through players
            foreach(KeyValuePair<int, Player> p in allPlayers)
            {
                // get random position
                String[] position = spawnPositions[counter].Split(';');

                // spawn player
                p.Value.setPosition(Convert.ToInt32(position[0]), Convert.ToInt32(position[1]));

                // count
                counter++;
            }

            // listen for move events
            this.com.onMove += new ComMessageHandler(MoveHandler);

            return true;
        }

        // HANDLER

        /// <summary>
        /// Handler for move events
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message"></param>
        public void MoveHandler(TcpClient client, String message)
        {
            // get player id
            int id = client.Client.GetHashCode();

            // new player coordinates
            int changex = 0;
            int changey = 0;

            // types
            switch (message)
            {
                case "n":
                    changey = -1;
                    break;
                case "e":
                    changex = 1;
                    break;
                case "s":
                    changey = 1;
                    break;
                case "w":
                    changex = -1;
                    break;
            }
            
            // positions
            int newx = this.ph.players[id].X + changex;
            int newy = this.ph.players[id].Y + changey;

            if (newx > 0 && newy > 0 && newy <= this.bomberMap.height && newx <= this.bomberMap.width )
            {
                // check maptile type from bombermap
                if (this.bomberMap.MapTiles[newy][newx].type != 0)
                {
                    // move
                    this.ph.players[id].setPosition(newx, newy);

                    if (this.bomberMap.MapTiles[newy][newx].type == 2) this.bomberMap.MapTiles[newy][newx].type = 1;
                    else if (this.bomberMap.MapTiles[newy][newx].type == 1) this.bomberMap.MapTiles[newy][newx].type = 2;
                    //Thread.Sleep(50);
                    this.bomberMap.sendMapToAll();
                }
            }
        }
        
    }
}
