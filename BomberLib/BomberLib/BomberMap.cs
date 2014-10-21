using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace BomberLib
{
    class BomberMap
    {
        // declaration
        public int height = 0;
        public int width = 0;
        public Dictionary<int, Dictionary<int, MapTile>> MapTiles = new Dictionary<int,Dictionary<int,MapTile>>();
        public String name = "";
        public String rawMap;

        private Communication com;

        // initialization: loads the map from file
        public BomberMap(String MapName, Communication c)
        {
            // save name
            this.name = MapName;

            // comclass
            this.com = c;

            // temporary map string
            String mapTemp = "";

            // load map file
            try
            {
                using (StreamReader sr = new StreamReader("maps/"+this.name+".txt"))
                {
                    String line = sr.ReadToEnd();
                    mapTemp += line;
                }
            }
            catch (Exception e)
            {
                // map doesn't exist?
                Console.WriteLine("Fatal Error! The map file couldn't be load: ");
                Console.WriteLine(e.Message);
            }

            // remove newlines (only for mapper, not relevant for lib)
            mapTemp = mapTemp.Replace(System.Environment.NewLine, "");

            // set raw map
            this.rawMap = mapTemp;

            // split map rows
            String[] rows = mapTemp.Split(';');

            // set up row counter
            int rowCounter = 0;

            // loop rows
            foreach (String row in rows)
            {
                // split tiles
                String[] tiles = row.Split(':');

                // check for empty rows
                if(tiles.Length > 1)
                {
                    // count rows
                    rowCounter++;

                    // set up tile counter
                    int tileCounter = 0;

                    // new temporary row dictionary
                    Dictionary<int, MapTile> rowTemp = new Dictionary<int, MapTile>();

                    // loop tiles
                    foreach (String tile in tiles)
                    {
                        // check for tile content
                        if (tile != "")
                        {
                            // count tiles
                            tileCounter++;

                            // add map tile to row dictionary
                            rowTemp.Add(tileCounter, new MapTile(Convert.ToInt32(tile.Trim()), rowCounter, tileCounter));
                        }
                    }

                    // set width
                    if(tileCounter > this.width) this.width = tileCounter;

                    // add temporary row dictionary to map dictionary
                    this.MapTiles.Add(rowCounter, rowTemp);
                }

                // set height
                this.height = rowCounter;
            }

            // output
            Console.WriteLine("Map '" + this.name + "' ("+this.width+"x"+this.height+") loaded");
            
            // start getmap listener
            this.com.onGetMap += new ComMessageHandler(GetMapHandler);

        }

        /// <summary>
        ///     Handle get map requests
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message"></param>
        public void GetMapHandler(TcpClient client, String message)
        {
            // return map to client
            sendMapTo(client);
        }

        // send the CURRENT map to a client
        public void sendMapTo(TcpClient client)
        {
            // get map
            String map = this.getCurrentMap();

            // send to client
            this.com.send(client, "Map", map);
        }

        /// <summary>
        ///     get the current map as string
        /// </summary>
        public String getCurrentMap()
        {
            // map string
            String map = "";

            // loop rows with height
            for (int row = 1; row <= this.height; row++ )
            {
                // loop throug tiles with width
                for (int tile = 1; tile <= this.width; tile++)
                {
                    // add tile to string
                    map += this.MapTiles[row][tile].type;

                    // add delimiter if necessary
                    if (tile < this.width) map += ":";
                }
                map += ";";
            }

            // return map
            return map;
        }

    }
}
