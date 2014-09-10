﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace BomberLib
{
    public class Client
    {
        // tcp 
        TcpClient client;
        NetworkStream clientStream;


        /// <summary>
        ///     Connects to a server.
        /// </summary>
        /// <param name="ip">The ip of the server.</param>
        /// <param name="port">The port of the server.</param>
        /// <returns>Success of connection.</returns>
        public Boolean connect(String ip, int port)
        {
            this.client = new TcpClient();

            try
            {
                // get server & connect
                IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
                this.client.Connect(serverEndPoint);

                // get stream
                this.clientStream = this.client.GetStream();

                // new thread
                Thread listener = new Thread(messageListener);

                // start listener
                listener.Start();

                return true;
            }
            catch (Exception e)
            {
                //Console.WriteLine(e.Message);
                return false;
            }


        }

        /// <summary>
        ///     Sends message to server
        /// </summary>
        /// <param name="message">The message to send to the server</param>
        /// <returns></returns>
        public Boolean send(String message)
        {

            // encode message
            ASCIIEncoding encoder = new ASCIIEncoding();
            byte[] buffer = encoder.GetBytes(message);

            // send
            this.clientStream.Write(buffer, 0, buffer.Length);
            this.clientStream.Flush();

            return false;
        }

        /// <summary>
        ///     Listener for messages from Server.
        /// </summary>
        private void messageListener()
        {
            byte[] message = new byte[4096];

            // forever...
            while (true)
            {
                int bytesRead = 0;
                try
                {
                    // wait for message
                    bytesRead = this.clientStream.Read(message, 0, 4096);
                }
                catch
                {
                    // error
                    break;
                }

                if (bytesRead == 0)
                {
                    // disconnect?
                    break;
                }

                // read message
                ASCIIEncoding encoder = new ASCIIEncoding();

                String get = encoder.GetString(message, 0, bytesRead);

                // noooooooot in final version of class please becausea ääääh
                /*if (get == "server_stop")
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Server has stopped");
                    Console.ResetColor();
                    Console.WriteLine("Press any key to exit!");
                    Console.ReadLine();
                    Environment.Exit(0);
                }

                // print to console (debug)
                Console.WriteLine(get);*/
            }
        }
    }
}
