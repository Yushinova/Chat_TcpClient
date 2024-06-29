using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tcp_Server_Console.Models;

namespace Tcp_Server_Console.Db_servise
{
    public class SQL_users
    {
        SQL_servise<User> sql_users = new SQL_servise<User>();
        public IEnumerable<User> GetAll()
        {
            return sql_users.GetValues("Users");
        }

        public User GetByID(int id)
        {
            return sql_users.GetById("Users", "Id_user", id);
        }

        public void InsertObj(User obj)//делаем запрос на вставку и передаем в сервис
        {
            string sql = $"insert into Users (Name,Login,Password,isActual) values (N'{obj.Name}','{obj.Login}', '{obj.Password}', {obj.isActual})";
            sql_users.UpdateAndInsert(sql);
        }
    }
}
