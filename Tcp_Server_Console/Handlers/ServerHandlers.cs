﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Tcp_Server_Console.Models;

namespace Tcp_Server_Console.Handlers
{
   public class ServerHandlers
    {
      public async Task RegistrationUser(NetworkStream stream)
        {
            Mappers.Mapper mapper = new Mappers.Mapper();
            Mappers.Mapper_dll mapper_dll = new Mappers.Mapper_dll();
            byte[] buffer = new byte[1024];
            int responce = await stream.ReadAsync(buffer, 0, buffer.Length);
            dll_tcp_chat.Deserialize_data<dll_tcp_chat.User_reg_dll> data = new dll_tcp_chat.Deserialize_data<dll_tcp_chat.User_reg_dll>();
            dll_tcp_chat.User_reg_dll user_Reg = data.GetObgFromBytes(buffer);
            Console.WriteLine(user_Reg.Login);
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
    }
}