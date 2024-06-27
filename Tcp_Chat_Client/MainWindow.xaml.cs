using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Tcp_Chat_Client
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public TcpClient tcpClient;
        public dll_tcp_chat.User_reg_dll user = new dll_tcp_chat.User_reg_dll();
        public MainWindow()
        {
            InitializeComponent();
            AuthorUser();
            
        }
        public async void RegistrUser()
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
                if (LoginText.Text != string.Empty && PasswordText.Text != string.Empty && NameText.Text != string.Empty)
                {
                    dll_tcp_chat.User_reg_dll user = new dll_tcp_chat.User_reg_dll()
                    {
                        Name = NameText.Text,
                        Login = LoginText.Text,
                        Password = PasswordText.Text
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
                        RegistrationGrid.Visibility = Visibility.Hidden;
                        MainGrid.Visibility = Visibility.Visible;
                        tcpClient.Close();
                        //Запрос на получение всех контактов
                    }
                    else
                    {
                        MessageBox.Show("Логин занят!");
                    }
                }

            }
            catch
            {
                MessageBox.Show("Нет связи с сервером!");
                tcpClient.Close();
            }

        }
        private void AuthorUser()
        {
            string path = Directory.GetCurrentDirectory();
            string[] dirs = Directory.GetFiles(path, "*.json");
            List<string> items = new List<string>();
            foreach (var item in dirs)
            {
                items.Add(System.IO.Path.GetFileName(item));
            }
            if (items.Count > 0)
            {
                AuthorPanel.Visibility = Visibility.Visible;
                AuthorUsers.ItemsSource = items;

            }
        }
        private void RegistrButton_Click(object sender, RoutedEventArgs e)
        {
            RegistrUser();
        }

        private void AuthorButton_Click(object sender, RoutedEventArgs e)
        {
            if(AuthorUsers.SelectedIndex!=-1)
            {
                if (File.Exists(AuthorUsers.SelectedItem.ToString()))
                {
                    using (StreamReader r = new StreamReader(AuthorUsers.SelectedItem.ToString()))
                    {
                        string json = r.ReadToEnd();
                        user = JsonSerializer.Deserialize<dll_tcp_chat.User_reg_dll>(json);
                    }
                    RegistrationGrid.Visibility = Visibility.Hidden;
                    AuthorPanel.Visibility = Visibility.Hidden;
                    MainGrid.Visibility = Visibility.Visible;
                    //авторизация уже сохраненных юзеров из файликов
                }
            }
           
        }
    }
}
