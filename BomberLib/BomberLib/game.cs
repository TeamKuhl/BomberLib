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
        BombHandler bombHandler;

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
            while (this.ph.getWaitingPlayerCount() < 1) { } // TODO Minplayers from config
            
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

            // counter for IENumerable 
            int counter = 0;

            // loop through players
            foreach(KeyValuePair<int, Player> p in ph.players)
            {

                // only spawn waiting & playing players, no spectators
                if (ph.players[p.Value.socketID].status <= 2)
                {
                    // get random position
                    String[] position = spawnPositions[counter].Split(';');

                    // set position
                    ph.players[p.Value.socketID].setStatus(1);

                    // spawn player
                    ph.players[p.Value.socketID].setPosition(Convert.ToInt32(position[0]), Convert.ToInt32(position[1]));

                    // count
                    counter++;
                }
            }

            // bombhandler
            this.bombHandler = new BombHandler(com);

            // listen for bombexplosions
            this.bombHandler.onBombExplosion += new BombExplosionHandler(BombExplosionHandler);

            // listen for bomb place
            this.com.onBombPlace += new ComMessageHandler(BombPlaceHandler);

            // listen for move events
            this.com.onMove += new ComMessageHandler(MoveHandler);

            return true;
        }

        // ==================
        // GAME EVENT HANDLER
        // ==================

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
                if (this.bomberMap.MapTiles[newy][newx].type == 1)
                {
                    // check if there is a player
                    if (!this.ph.isPlayerOnPosition(newx, newy))
                    {
                        // check for bomb
                        if (!this.bombHandler.isBombAtPosition(newx, newy))
                        {
                            // move
                            if (this.ph.players[id].setPosition(newx, newy))
                            {
                                // do actions yeah
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// handles bomb explosions
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="size"></param>
        public void BombExplosionHandler(int X, int Y, int size)
        {
            // bomb location
            if (ph.isPlayerOnPosition(X, Y))
            {
                Player p = ph.getPlayerOnPosition(X, Y);
                p.die();
            }

            // explosion locations (starting with X,Y)
            String locations = X + ":" + Y + ";";

            // loop bomb positions
            for (int g = 1; g <= 4; g++)
            {

                // loop size
                for(int i = 1; i<=size; i++)
                {
                    // calculate explosion positions
                    int locX = X;
                    int locY = Y;

                    // all 4 directions
                    switch(g)
                    {
                        case 1:
                            locY = Y - i;
                            break;
                        case 2:
                            locX = X + i;
                            break;
                        case 3:
                            locY = Y + i;
                            break;
                        case 4:
                            locX = X - i;
                            break;
                    }

                    if (this.bomberMap.MapTiles.ContainsKey(locY) && this.bomberMap.MapTiles[locY].ContainsKey(locX))
                    {

                        // check if wall, Karol <3
                        Boolean NichtIstWand = true;

                        // tiletypes
                        switch (this.bomberMap.MapTiles[locY][locX].type)
                        {
                            case 0:
                                // wall
                                NichtIstWand = false;
                                break;
                            case 1:
                                // add to exploded locations
                                locations += locX + ":" + locY + ";";

                                // check for player
                                if (ph.isPlayerOnPosition(locX, locY))
                                {
                                    Player p = ph.getPlayerOnPosition(locX, locY);
                                    p.die();
                                }
                                break;
                            case 2:
                                // add to exploded locations
                                locations += locX + ":" + locY + ";";

                                // breakable
                                this.bomberMap.MapTiles[locY][locX].type = 1;
                                this.bomberMap.sendMapToAll();

                                // end explosion
                                NichtIstWand = false;

                                break;
                        }

                        if (!NichtIstWand) break;
                    }
                }
            }

            // send exploded locations to all clients
            this.com.sendToAll("BombExploded", locations);
        }

        /// <summary>
        /// Handles bomb place events
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message"></param>
        public void BombPlaceHandler(TcpClient client, String message)
        {
            // get player
            Player p = this.ph.getPlayerByHashcode(client.Client.GetHashCode());

            // place bomb
            this.bombHandler.placeBomb(p.size, p.time, p.X, p.Y);
            
        }
        
    }
}
