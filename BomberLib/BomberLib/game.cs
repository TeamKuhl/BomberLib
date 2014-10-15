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
        public Boolean start()
        {
            // load config
            Config config = new Config();

            // load map
            BomberMap bomberMap = new BomberMap();

            // create server
            Server server = new Server();
            
            // start server
            server.start(45454);

            // create communication object
            Communication com = new Communication(server);
                        
            return true;
        }
        
    }
}
