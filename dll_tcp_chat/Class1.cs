using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices.ComTypes;

namespace dll_tcp_chat
{
    public class USED_CODES
    {
        public const string AUTHORIZATION_REQUEST = "A001";//запрос авторизации
        public const string REGISTRATION_REQUEST = "R001";//запрос регистрации
        public const string SEND_MESSAGE = "S002";//запрос отправки сообщения
        public const string RECEIVE_MESSAGES = "R002";//запрос приема всех своих сообщений
        public const string RECEIVE_USERS = "R003";

    }
    public class USED_ERRORS
    {
        public const string REGISTRATION_ERROR = "401";
        public const string AUTHORIZATION_ERROR = "400";
        public const string SERVER_ERROR = "404";//сервер недоступен
        public const string GOOD_CODE = "200";//OK
    }

    public class Deserialize_data<T>
    {
        BinaryFormatter formatter = new BinaryFormatter();
        //methods: получить лист юзеров
        public T GetObgFromBytes(byte[] bytes)
        {
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                formatter = new BinaryFormatter();
                T obj = (T)formatter.Deserialize(stream);
                return obj;
            }
        }
        //получить лист сообщений
        public List<T> GetListFromBytes(byte[] bytes)
        {
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                List<T> list = new List<T>();
                formatter = new BinaryFormatter();
                list = (List<T>)formatter.Deserialize(stream);
                return list;
            }
        }
        //получить сообщение
        //получить юзера
    }
    public class Serialize_data<T>
    {
        BinaryFormatter formatter = new BinaryFormatter();
        public byte[] GetBytesFromList(List<T> list)
        {
            using (MemoryStream stream = new MemoryStream())//переводим все рецепты в байты
            {
                formatter = new BinaryFormatter();
                formatter.Serialize(stream, list);
                byte[] byffer = stream.ToArray();
                return byffer;
            }

        }
        public byte[] GetBytesFromObj(T obj)
        {
            using (MemoryStream stream = new MemoryStream())//переводим все рецепты в байты
            {
                formatter = new BinaryFormatter();
                formatter.Serialize(stream, obj);
                byte[] byffer = stream.ToArray();
                return byffer;
            }
        }
    }

    [Serializable]
    public class Attachment_dll
    {
        public string FileName { get; set; }
        public byte[] Body { get; set; }
        public Attachment_dll(string path)//строка из сообщения путь
        {
            FileName = Path.GetFileName(path);
            Body =  File.ReadAllBytes(path);
        }
    }

    [Serializable]
    public class Message_dll
    {
        public int Id_to { get; set; }
        public int Id_from { get; set; }
        public string Text { get; set; }
        public DateTime Time_send { get; set; }
        public Attachment_dll Attachment { get; set; }
    }

    [Serializable]

    public class User_reg_dll//передача для авторизации регистрации со стороны сервера
    {
        public int Id_user { get; set; }
        public string Name { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
    }
    [Serializable]
    public class User_dll//для получения всех юзеров
    {
        public int Id_user { get; set; }
        //public string Login { get; set; }//до собаки чтобы понятно было что за юзер
        public string Name { get; set; }
    }


}
