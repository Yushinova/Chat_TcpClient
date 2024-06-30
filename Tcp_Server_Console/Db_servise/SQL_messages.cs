using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tcp_Server_Console.Models;

namespace Tcp_Server_Console.Db_servise
{
    public class SQL_messages
    {
        SQL_servise<Message> sql_messages = new SQL_servise<Message>();
        public IEnumerable<Message> GetAll()
        {
            return sql_messages.GetValues("Messages");
        }
        public IEnumerable<Message> GetAllById(int id)
        {
            return sql_messages.GetAllById("Messages", "Id_to_user", id);
        }
        public Message GetByID(int id)
        {
            return sql_messages.GetById("Messages", "Id_to_user", id);
        }

        public void InsertObj(Message obj)//делаем запрос на вставку и передаем в сервис
        {
            string sql = $"insert into Messages (Text,Attachment_path,Id_to_user,Id_from_user,Time_send) values (N'{obj.Text}',N'{obj.Attachment_path}', {obj.Id_to_user}, {obj.Id_from_user}, '{obj.Time_send}')";
            sql_messages.UpdateAndInsert(sql);
        }
    }
}
