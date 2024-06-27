using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Tcp_Server_Console.Db_servise;
using Tcp_Server_Console.Models;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Net.Http;

namespace Tcp_Server_Console
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Handlers.ServerHandlers handlers = new Handlers.ServerHandlers();
            var tcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), 1024);
            Handlers.ServerHandlers hadler = new Handlers.ServerHandlers();
            List<User> users = new List<User>();
            List<Message> messages = new List<Message>();
            try
            {
                tcpListener.Start();    // запускаем сервер

                await Task.Run(() => ReceiveSend());

            }
            finally
            {
                tcpListener.Stop();
            }
            async Task ReceiveSend()
            {
                while (true)
                {
                    
                    Console.WriteLine("Сервер запущен. Ожидание подключений... ");
                    var client = await tcpListener.AcceptTcpClientAsync();
                    
                    //получаем листы юзеров и сообщений
                    NetworkStream stream = client.GetStream();//обработка кодов запросов нужен трай кеч
                    byte[] byffer = new byte[1024];
                    int responce = await stream.ReadAsync(byffer, 0, byffer.Length);
                    var message = Encoding.UTF8.GetString(byffer, 0, responce);
                    Console.WriteLine(message);
                    switch (message)
                    {
                        case dll_tcp_chat.USED_CODES.REGISTRATION_REQUEST://регистрация
                         await handlers.RegistrationUser(stream);
                            break;
                        case dll_tcp_chat.USED_CODES.AUTHORIZATION_REQUEST://авторизация                          
                          
                            break;
                        case dll_tcp_chat.USED_CODES.SEND_MESSAGE://отправить сообщение

                            break;
                        case dll_tcp_chat.USED_CODES.RECEIVE_USERS://получить все контакты

                            break;
                        case dll_tcp_chat.USED_CODES.RECEIVE_MESSAGES://получить сообщения

                            break;
                        default:
                            Console.WriteLine("НЕИЗВЕСТНЫЙ ЗАПРОС!");
                            break;

                    }
                }

            }
        }
    }
}
