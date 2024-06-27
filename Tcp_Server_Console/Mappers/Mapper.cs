using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tcp_Server_Console.Models;

namespace Tcp_Server_Console.Mappers
{
    public class Mapper
    {
        string FilePath = AppDomain.CurrentDomain.BaseDirectory.ToString() + "\\Folder\\";//путь к папке bin
        public User MapUserRegDllToUser(dll_tcp_chat.User_reg_dll user_dll)
        {
            return new User()
            {
                Name = user_dll.Name,
                Login = user_dll.Login,
                Password = user_dll.Password,
                isActual = 1
            };
        }
        public Message MapMessageDlToMessage(dll_tcp_chat.Message_dll message_dll)
        {
            //try
            //{
            //    File.WriteAllBytes(FilePath + message_dll.Attachment.FileName, message_dll.Attachment.Body);
            //}
            //catch (Exception ex) { }

            return new Message()
            {
                Attachment_path = FilePath + message_dll.Attachment.FileName,//надо сохранять вложение и брать этот путь
                Id_from_user = message_dll.Id_from,
                Id_to_user = message_dll.Id_to,
                Text = message_dll.Text,
                Time_send = message_dll.Time_send
            };
        }
    }

    public class Mapper_dll
    {
        public dll_tcp_chat.User_dll MapUserToUserDll(User user)
        {
            return new dll_tcp_chat.User_dll()
            {
                Id_user = user.Id_user,
                Name = user.Name
            };
        }

        public dll_tcp_chat.User_reg_dll MapUserToUserRegDll(User user)
        {
            return new dll_tcp_chat.User_reg_dll()
            {
                Id_user = user.Id_user,
                Name = user.Name,
                Login = user.Login,
                Password = user.Password
            };
        }

        public dll_tcp_chat.Message_dll MapMessageToMessageDll(Message message)
        {
            return new dll_tcp_chat.Message_dll()
            {
                Attachment = new dll_tcp_chat.Attachment_dll(message.Attachment_path),
                Id_from = message.Id_from_user,
                Id_to = message.Id_to_user,
                Text = message.Text,
                Time_send = message.Time_send
            };
        }
    }
}
