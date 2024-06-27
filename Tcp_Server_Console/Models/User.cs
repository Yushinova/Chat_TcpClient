using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tcp_Server_Console.Models
{
    public class User//юзер базы данных
    {
        public int Id_user { get; set; }
        public string Name { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public int isActual { get; set; }//при удалении юзера он становиттся не актуальным
    }
}
