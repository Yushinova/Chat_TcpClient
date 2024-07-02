using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Tcp_Chat_Client.WPF_servise
{
    public class Servise
    {
        public async Task<dll_tcp_chat.User_reg_dll> RegistrUser(string login, string password, string name)//регистрация
        {
            try
            {
                TcpClient tcpClient = new TcpClient();
                await tcpClient.ConnectAsync("127.0.0.1", 1024);
                dll_tcp_chat.Serialize_data<dll_tcp_chat.User_reg_dll> serialize = new dll_tcp_chat.Serialize_data<dll_tcp_chat.User_reg_dll>();
                dll_tcp_chat.Deserialize_data<dll_tcp_chat.User_reg_dll> deserialize = new dll_tcp_chat.Deserialize_data<dll_tcp_chat.User_reg_dll>();
                NetworkStream stream = tcpClient.GetStream();
                //запрос на регистрацию
                var message = dll_tcp_chat.USED_CODES.REGISTRATION_REQUEST;
                var dateTimeBytes = Encoding.UTF8.GetBytes(message);
                await stream.WriteAsync(dateTimeBytes, 0, dateTimeBytes.Length);
                //получаем юзера из формы регистрации
                if (login != string.Empty && password != string.Empty && name != string.Empty)
                {
                    dll_tcp_chat.User_reg_dll user = new dll_tcp_chat.User_reg_dll()
                    {
                        Name = name,
                        Login = login,
                        Password = password
                    };
                    //передаем юзера серверу
                    byte[] bytes = serialize.GetBytesFromObj(user);
                    await stream.WriteAsync(bytes, 0, bytes.Length);
                    //получаем ответ по регистрации
                    byte[] byffer = new byte[1024];
                    int responce = await stream.ReadAsync(byffer, 0, byffer.Length);
                    string ansver = Encoding.UTF8.GetString(byffer, 0, byffer.Length);
                    //получен ответ удачной регистрации
                    if (ansver.Contains(dll_tcp_chat.USED_ERRORS.GOOD_CODE))
                    {
                        // LoginText.Text = Encoding.UTF8.GetString(byffer, 0, byffer.Length);
                        //при удачной регистрации сервер присылает юзера уже из базы данных с ID
                        responce = await stream.ReadAsync(byffer, 0, byffer.Length);
                        user = deserialize.GetObgFromBytes(byffer);
                        string fileName = $"{user.Login}.json";//пишем его в файл

                        using (FileStream createStream = new FileStream(fileName, FileMode.OpenOrCreate))
                        {
                            JsonSerializer.Serialize<dll_tcp_chat.User_reg_dll>(createStream, user);
                        }
                        MessageBox.Show("Вы успешно зарегистрированы!");
                        tcpClient.Close();
                        return user;
                    }
                    else
                    {
                        MessageBox.Show("Логин занят!");
                        return null;
                    }
                }
                return null;
            }
            catch
            {
               // MessageBox.Show("Нет связи с сервером!");
                return null;
            }
        }
        public async Task<List<dll_tcp_chat.User_dll>> GetAllUsers()//получение всех контактов
        {
            try
            {
                TcpClient tcpClient = new TcpClient();
                await tcpClient.ConnectAsync("127.0.0.1", 1024);
                dll_tcp_chat.Serialize_data<dll_tcp_chat.User_dll> serialize = new dll_tcp_chat.Serialize_data<dll_tcp_chat.User_dll>();
                dll_tcp_chat.Deserialize_data<dll_tcp_chat.User_dll> deserialize = new dll_tcp_chat.Deserialize_data<dll_tcp_chat.User_dll>();
                NetworkStream stream = tcpClient.GetStream();
                //запрос на регистрацию
                var message = dll_tcp_chat.USED_CODES.RECEIVE_USERS;
                var date = Encoding.UTF8.GetBytes(message);
                await stream.WriteAsync(date, 0, date.Length);

                //ожидаем юзеров

                int bytes;  // количество полученных байтов
                byte[] byffer = new byte[1024];
                byte[] all_butes = new byte[0];
                do
                {
                    //получаем данные
                    bytes = await stream.ReadAsync(byffer, 0, byffer.Length);//мало ли сколько их там
                    // склеивем массивы байтов
                    all_butes = all_butes.Concat(byffer).ToArray();
                }
                while (stream.DataAvailable); // пока данные есть в потоке 
                List<dll_tcp_chat.User_dll> users = deserialize.GetListFromBytes(all_butes);
                tcpClient.Close();
                return users;
            }
            catch
            {
               // MessageBox.Show("Нет связи с сервером!");
                return null;
            }
        }

        public async Task<List<dll_tcp_chat.Message_dll>> GetAllMessage(int Id)//получение всех сообщений
        {
            try
            {
                TcpClient tcpClient = new TcpClient();
                await tcpClient.ConnectAsync("127.0.0.1", 1024);
                dll_tcp_chat.Deserialize_data<dll_tcp_chat.Message_dll> deserialize = new dll_tcp_chat.Deserialize_data<dll_tcp_chat.Message_dll>();
                NetworkStream stream = tcpClient.GetStream();
                //запрос на регистрацию
                var message = dll_tcp_chat.USED_CODES.RECEIVE_MESSAGES;
                var date = Encoding.UTF8.GetBytes(message);
                await stream.WriteAsync(date, 0, date.Length);
                //отправить ID user
                date = Encoding.UTF8.GetBytes(Id.ToString());
                await stream.WriteAsync(date, 0, date.Length);
                //ожидаем messages
                int bytes;  // количество полученных байтов
                byte[] byffer = new byte[100000];//для отправки больших файлов нужен большой буфер
                byte[] all_butes = new byte[0];
                do
                {
                    //получаем данные
                    bytes = await stream.ReadAsync(byffer, 0, byffer.Length);//мало ли сколько их там
                    // склеивем массивы байтов
                    all_butes = all_butes.Concat(byffer).ToArray();
                }
                while (stream.DataAvailable); // пока данные есть в потоке 
                List<dll_tcp_chat.Message_dll> messages = deserialize.GetListFromBytes(all_butes);
                tcpClient.Close();
                return messages;
            }
            catch
            {
                //MessageBox.Show("Нет связи с сервером!");
                return null;
            }
        }
        public async Task SendMessage(dll_tcp_chat.Message_dll message)//отправка сообщения
        {
            try
            {
                TcpClient tcpClient = new TcpClient();
                await tcpClient.ConnectAsync("127.0.0.1", 1024);
                dll_tcp_chat.Serialize_data<dll_tcp_chat.Message_dll> serialize = new dll_tcp_chat.Serialize_data<dll_tcp_chat.Message_dll>();
                NetworkStream stream = tcpClient.GetStream();
                //запрос на регистрацию
                string message_code = dll_tcp_chat.USED_CODES.SEND_MESSAGE;
                byte[] date = Encoding.UTF8.GetBytes(message_code);
                await stream.WriteAsync(date, 0, date.Length);

                byte[] bytes = serialize.GetBytesFromObj(message);
                await stream.WriteAsync(bytes, 0, bytes.Length);
                tcpClient.Close();
            }
           catch
            {
               // MessageBox.Show("Нет связи с сервером!");
            }
        }
    }
}
