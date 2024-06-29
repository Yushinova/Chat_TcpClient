using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tcp_Server_Console.Models
{
    public class Message//сообщение базы данных
    {
        public int Id_message { get; set; }
        public string Text { get; set; }
        public string Attachment_path { get; set; }
        public int Id_to_user { get; set; }
        public int Id_from_user { get; set; }
        public string Time_send { get; set; }
    }
}
