using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tcp_Server_Console.Models;

namespace Tcp_Server_Console.Db_servise
{
    public class SQL_servise<T>
    {
        private readonly SqlConnection _db;
        public SQL_servise()//подключение к бд
        {
            Config config = new Config();
            var connectionstr = config.GetFromTXT();
            if (string.IsNullOrEmpty(connectionstr))
            {
                throw new ArgumentException($"Ошибка строки подключения {connectionstr}");
            }
            else { _db = new SqlConnection(connectionstr); }
        }
       
        public IEnumerable<T> GetValues(string table_name)
        {
            _db.Open();
            var sql = $"Select * from {table_name}";
            IEnumerable<T> values = _db.Query<T>(sql);
            _db.Close();
            return values;
        }
        public IEnumerable<T> GetAllById(string table_name, string column_name, int key)
        {
            _db.Open();
            var sql = $"Select * from {table_name} where {column_name} = {key}";
            IEnumerable<T> values = _db.Query<T>(sql);
            _db.Close();
            return values;
        }
        public T GetById(string table_name, string column_name, int key)//не нужно пока
        {
            _db.Open();
            var sql = $"Select * from {table_name} where {column_name} = {key}";
            T value = _db.QuerySingle<T>(sql);
            _db.Close();
            return value;

        }

        public void UpdateAndInsert(string sql)
        {
            _db.Open();
            _db.Execute(sql);
            _db.Close();
        }
    }
}
