using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunicationLibrary;

namespace BomberLib
{
    class Game
    {
        

        public Boolean start()
        {
            // load config
            Config config = new Config();

            // load map
            BomberMap bomberMap = new BomberMap();

            // create server
            Server server = new Server();

            // start server
            server.start(Convert.ToInt32(config.get("server-port")));

            return false;
        }
    }
}
