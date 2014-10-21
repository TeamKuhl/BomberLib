using System;
using System.Collections.Generic;
using System.Linq;
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
            this.newRound();
                        
            return true;
        }

        public Boolean newRound()
        {


            return true;
        }
        
    }
}
