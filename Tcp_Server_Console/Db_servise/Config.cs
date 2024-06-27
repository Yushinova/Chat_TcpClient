using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tcp_Server_Console.Db_servise
{
    public class Config
    {
        public string connection_str { get; set; }

        public string GetFromTXT(string path = "Db_servise\\Config_str.txt")
        {
            using (StreamReader reader = new StreamReader(path))
            {
                connection_str = reader.ReadToEnd();
            }
          
            return connection_str;
           
        }
    }
}
