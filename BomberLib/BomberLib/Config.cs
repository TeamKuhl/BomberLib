using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BomberLib
{
    public class Config
    {
        public Dictionary<String, String> settings = new Dictionary<string, string>();

        /// <summary>
        ///     loads the config from file 
        /// </summary>
        public Config()
        {
            // testing
            this.set("server-port", "45454");
            // this.initialize();
        }



        /// <summary>
        ///     set a setting
        /// </summary>
        /// <param name="name">name of setting</param>
        /// <param name="value">value of setting</param>
        public void set(string name, string value)
        {
            if (settings.ContainsKey(name))
            {
                settings[name] = value;
            }
            else
            {
                settings.Add(name, value);
            }
        }

        /// <summary>
        ///     get a setting
        /// </summary>
        /// <param name="name">name of setting</param>
        /// <returns></returns>
        public string get(string name)
        {
            if (settings.ContainsKey(name))
            {
                if (settings[name].Trim() == "")
                {
                    return settings[name].Trim();
                }
                else return null;
            }
            else return null;
        }
    }
}
