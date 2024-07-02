using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Tcp_Server_Console.Models;

namespace Tcp_Server_Console.Handlers
{
   public class ServerHandlers
    {
       public  Mappers.Mapper mapper = new Mappers.Mapper();
       public Mappers.Mapper_dll mapper_dll = new Mappers.Mapper_dll();
        public async Task RegistrationUser(NetworkStream stream)//запрос на регистрацию
        {
           
            byte[] buffer = new byte[1024];
            int responce = await stream.ReadAsync(buffer, 0, buffer.Length);
            dll_tcp_chat.Deserialize_data<dll_tcp_chat.User_reg_dll> data = new dll_tcp_chat.Deserialize_data<dll_tcp_chat.User_reg_dll>();
            dll_tcp_chat.User_reg_dll user_Reg = data.GetObgFromBytes(buffer);
           // Console.WriteLine(user_Reg.Login);
            //запрос юзеров из базы данных
            Db_servise.SQL_users users_db = new Db_servise.SQL_users();
            List<User> users = users_db.GetAll().ToList();
            //если пользователь уже есть с таким логином отправляем ошибку
            if(users.Any(u=>u.Login==user_Reg.Login))
            {
                buffer = Encoding.UTF8.GetBytes(dll_tcp_chat.USED_ERRORS.REGISTRATION_ERROR);
                await stream.WriteAsync(buffer, 0, buffer.Length);
            }
            else
            {
                users_db.InsertObj(mapper.MapUserRegDllToUser(user_Reg));//добавление пользователя в базу данных
                buffer = Encoding.UTF8.GetBytes(dll_tcp_chat.USED_ERRORS.GOOD_CODE);
                await stream.WriteAsync(buffer, 0, buffer.Length);

                users = users_db.GetAll().ToList();//получаем юзера уже из базы данных с правильным ID
                user_Reg = mapper_dll.MapUserToUserRegDll(users.First(u => u.Login == user_Reg.Login));
                dll_tcp_chat.Serialize_data<dll_tcp_chat.User_reg_dll> serialize = new dll_tcp_chat.Serialize_data<dll_tcp_chat.User_reg_dll>();
                buffer = serialize.GetBytesFromObj(user_Reg);
                await stream.WriteAsync(buffer, 0, buffer.Length);
            }
        }
        public async Task SendAllUsers(NetworkStream stream)//отправляем все контакты из базы
        {
            Db_servise.SQL_users users_db = new Db_servise.SQL_users();
            List<User> users = users_db.GetAll().ToList();
            //Console.WriteLine(users.Count);
            List<dll_tcp_chat.User_dll> users_dll = new List<dll_tcp_chat.User_dll>();
            foreach (var user in users) 
            {
                users_dll.Add(mapper_dll.MapUserToUserDll(user));
            }
            //Console.WriteLine($"dll+users {users_dll.Count}");
            dll_tcp_chat.Serialize_data<dll_tcp_chat.User_dll> serialize = new dll_tcp_chat.Serialize_data<dll_tcp_chat.User_dll>();
            byte[] buffer = serialize.GetBytesFromList(users_dll);
           // Console.WriteLine(buffer.Length);
            await stream.WriteAsync(buffer, 0, buffer.Length);
        }

        public async Task SaveMessage(NetworkStream stream)//сохранение сообщения в базе
        {
            Db_servise.SQL_messages save_message = new Db_servise.SQL_messages();
            dll_tcp_chat.Deserialize_data<dll_tcp_chat.Message_dll> deserialize = new dll_tcp_chat.Deserialize_data<dll_tcp_chat.Message_dll>();
            Mappers.Mapper mapper = new Mappers.Mapper();
            int bytes;  // количество полученных байтов
            byte[] byffer = new byte[10000];
            byte[] all_butes = new byte[0];
            do
            {
                //получаем данные
                bytes = await stream.ReadAsync(byffer, 0, byffer.Length);
                //Console.WriteLine(bytes);
                all_butes = all_butes.Concat(byffer).ToArray();
            }
            while (stream.DataAvailable); // пока данные есть в потоке 

            dll_tcp_chat.Message_dll message = deserialize.GetObgFromBytes(all_butes);//десериализует
           // Console.WriteLine($"{message.Attachment}");
            save_message.InsertObj(mapper.MapMessageDlToMessage(message));//в базу вставляет
            if (message.Attachment!=null)
            {
                File.WriteAllBytes($"Folder\\{message.Attachment.FileName}", message.Attachment.Body);//в папку сохраняет
            }
        }
        public async Task SendAllMessage(NetworkStream stream)
        {
            byte[] byffer = new byte[1024];
            int responce = await stream.ReadAsync(byffer, 0, byffer.Length);
            int Id = int.Parse(Encoding.UTF8.GetString(byffer, 0, responce));
            //Console.WriteLine($"Id user={Id}");
            Db_servise.SQL_messages messages_db= new Db_servise.SQL_messages();
            List<Message> messages = messages_db.GetAllById(Id).ToList();
           // Console.WriteLine($"Messages from db={messages.Count}");
 
            List<dll_tcp_chat.Message_dll> messages_dll = new List<dll_tcp_chat.Message_dll>();
            foreach (var message in messages)
            {
                messages_dll.Add(mapper_dll.MapMessageToMessageDll(message));
            }
            //Console.WriteLine($"dll+users {users_dll.Count}");
            dll_tcp_chat.Serialize_data<dll_tcp_chat.Message_dll> serialize = new dll_tcp_chat.Serialize_data<dll_tcp_chat.Message_dll>();
            byte[] buffer = serialize.GetBytesFromList(messages_dll);
           // Console.WriteLine(buffer.Length);
            await stream.WriteAsync(buffer, 0, buffer.Length);
        }
    }
}
