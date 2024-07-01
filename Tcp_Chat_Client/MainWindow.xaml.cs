using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Threading;
using Tcp_Chat_Client.WPF_servise;
using static System.Net.Mime.MediaTypeNames;
using static System.Net.WebRequestMethods;

namespace Tcp_Chat_Client
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string path = Directory.GetCurrentDirectory();
        public TcpClient tcpClient;
        public dll_tcp_chat.User_reg_dll user = new dll_tcp_chat.User_reg_dll();
        public List<dll_tcp_chat.User_dll> users = new List<dll_tcp_chat.User_dll>();
        public List<dll_tcp_chat.Message_dll> messages = new List<dll_tcp_chat.Message_dll>();
        public WPF_servise.Servise servise = new WPF_servise.Servise();
        public dll_tcp_chat.Attachment_dll attachment;
        public string message_file_name;
        dll_tcp_chat.Serialize_data<dll_tcp_chat.Message_dll> serialize = new dll_tcp_chat.Serialize_data<dll_tcp_chat.Message_dll>();
        public MainWindow()
        {
            InitializeComponent();

            AuthorUser();//авторизация по уже сохраненным данным, если уже входил ранее
        }
        private void dispatcherTimer_Tick(object sender, EventArgs e)//для таймера подвешиваем метод проверки новых сообщений
        {
            SetMessages();
        }
        private void AuthorUser()//устанавливаем для выбора юзеров, которые уже входили с этого компьютера
        {
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
        public Border NewMessagePanel(dll_tcp_chat.Message_dll message, string Name, bool isNew)//если новое выделить цветом
        {
            Border border = new Border();
            border.Background = Brushes.White;
            border.MouseDown += MouseDown_Messages;
            if (isNew) border.BorderBrush = Brushes.LightPink;
            else border.BorderBrush = Brushes.AliceBlue;
            border.BorderThickness = new Thickness(5, 5, 5, 5);
            StackPanel panel = new StackPanel();
            panel.Width = 400;
            panel.Orientation = Orientation.Vertical;
            TextBlock user_from_text = new TextBlock();
            user_from_text.Text = $"[{Name}] {message.Time_send.ToString()}";
            panel.Children.Add(user_from_text);
            TextBlock message_text = new TextBlock();
            message_text.Text = message.Text;
            panel.Children.Add(message_text);
            if (message.Attachment != null)//если есть вложение добавляем панель с кнопкой для скачивания
            {
                StackPanel attach_panel = new StackPanel();
                attach_panel.Orientation = Orientation.Horizontal;
                attach_panel.HorizontalAlignment = HorizontalAlignment.Right;
                attach_panel.Margin = new Thickness(0, 0, 10, 0);
                TextBlock attach_text = new TextBlock();
                attach_text.Text = System.IO.Path.GetExtension(message.Attachment.FileName);//расширение
                attach_panel.Children.Add(attach_text);
                Button button = new Button();
                button.Content = "Download";
                button.Click += DownloadFile_Click;//подвешиваем обработчик начатия на кнопку скачать
                attach_panel.Children.Add(button);
                panel.Children.Add(attach_panel);
                TextBlock index_message = new TextBlock();
                index_message.Text = message.Id.ToString();
                index_message.Visibility = Visibility.Hidden;//надо передать индекс сообщения
                attach_panel.Children.Add(index_message);
            }
            border.Child = panel;
            return border;
        }

        private async void SetMessages()//проверка и установка новых сообщений
        {
            List<dll_tcp_chat.Message_dll> message_from_DB = await servise.GetAllMessage(user.Id_user);
            if (messages.Count > 0)
            {
                for (int i = 0; i < message_from_DB.Count; i++)
                {
                    if (!messages.Any(m => m.Id == message_from_DB[i].Id))
                    {
                        messages.Add(message_from_DB[i]);
                        Dispatcher.Invoke(new Action(() => MessagePanel.Items.Add(NewMessagePanel(message_from_DB[i], users.First(u => u.Id_user == message_from_DB[i].Id_from).Name, true))));
                    }
                }
                using (FileStream createStream = new FileStream(message_file_name, FileMode.OpenOrCreate))
                {
                    byte[] jsonUtf8Bytes = serialize.GetBytesFromList(messages);
                    await JsonSerializer.SerializeAsync<byte[]>(createStream, jsonUtf8Bytes);//записываем в файл байты
                }
            }
            else
            {
                messages = message_from_DB;
                using (FileStream createStream = new FileStream(message_file_name, FileMode.OpenOrCreate))
                {
                    byte[] jsonUtf8Bytes = serialize.GetBytesFromList(messages);
                    await JsonSerializer.SerializeAsync<byte[]>(createStream, jsonUtf8Bytes);//записываем в файл байты
                }
                foreach (var item in message_from_DB)
                {
                    Dispatcher.Invoke(new Action(() => MessagePanel.Items.Add(NewMessagePanel(item, users.First(u => u.Id_user == item.Id_from).Name, true))));
                }
            }

        }
        private void DownloadFile_Click(object sender, RoutedEventArgs e)//скачать файл вложения сообщения
        {
            int index = int.Parse((((sender as Button).Parent as StackPanel).Children[2] as TextBlock).Text);
            dll_tcp_chat.Attachment_dll save_attachment = messages.First(m => m.Id == index).Attachment;
            var dialog = new Microsoft.Win32.SaveFileDialog();//диалоговое окно
            dialog.Filter = $"file|*.{System.IO.Path.GetExtension(save_attachment.FileName)}*";
            dialog.FileName = save_attachment.FileName;
            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                System.IO.File.WriteAllBytes(dialog.FileName, save_attachment.Body);
            }
        }
        private async void RegistrButton_Click(object sender, RoutedEventArgs e)
        {
            user = await servise.RegistrUser(LoginText.Text, PasswordText.Text, NameText.Text);
            users = await servise.GetAllUsers();
            if (users != null)
            {
                users.Remove(users.First(u => u.Login == user.Login));//убираем из контактов самого себя
                UsersList.ItemsSource = users;
                message_file_name = path + "\\Messages\\" + user.Login + "message.json";
            }
            if (user != null)
            {
                RegistrationGrid.Visibility = Visibility.Hidden;
                MainGrid.Visibility = Visibility.Visible;
            }
        }

        private async void AuthorButton_Click(object sender, RoutedEventArgs e)
        {
            if (AuthorUsers.SelectedIndex != -1)
            {
                if (System.IO.File.Exists(AuthorUsers.SelectedItem.ToString()))
                {
                    using (StreamReader r = new StreamReader(AuthorUsers.SelectedItem.ToString()))
                    {
                        var json = r.ReadToEndAsync();
                        user = JsonSerializer.Deserialize<dll_tcp_chat.User_reg_dll>(await json);
                        message_file_name = path + "\\Messages\\" + user.Login + "message.json";
                    }
                    UserPanel.DataContext = user;
                    users = await servise.GetAllUsers();
                    if (users != null && users.Count > 0)
                    {
                        await Task.Factory.StartNew(() =>//запуск проверки сообщений
                        {
                            System.Timers.Timer t = new System.Timers.Timer();
                            t.Interval = 5000;
                            t.Elapsed += dispatcherTimer_Tick;
                            t.Start();
                        });
                        users.Remove(users.First(u => u.Login == user.Login));//убираем из контактов самого себя
                        UsersList.ItemsSource = users;
                        RegistrationGrid.Visibility = Visibility.Hidden;
                        AuthorPanel.Visibility = Visibility.Hidden;
                        MainGrid.Visibility = Visibility.Visible;
                    }
                    if (System.IO.File.Exists(message_file_name))//записанные сообщения юзера
                    {
                        dll_tcp_chat.Deserialize_data<dll_tcp_chat.Message_dll> deserialize = new dll_tcp_chat.Deserialize_data<dll_tcp_chat.Message_dll>();

                        using (StreamReader r = new StreamReader(message_file_name))
                        {
                            var json = r.ReadToEndAsync();
                            byte[] bytes = JsonSerializer.Deserialize<byte[]>(await json);
                            messages = deserialize.GetListFromBytes(bytes);
                            foreach (var item in messages)
                            {
                                if(item.Id_from==user.Id_user)
                                {
                                    Dispatcher.Invoke(new Action(() => MessagePanel.Items.Add(NewMessagePanel(item, user.Name, false))));
                                }
                                else
                                {
                                    Dispatcher.Invoke(new Action(() => MessagePanel.Items.Add(NewMessagePanel(item, users.First(u => u.Id_user == item.Id_from).Name, false))));
                                }                              
                            }
                        }
                    }
                }
            }
           
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)//отправка сообщения
        {
            if (UserToText.Text != string.Empty)
            {
                dll_tcp_chat.Message_dll message = new dll_tcp_chat.Message_dll();
                message.Text = MessageText.Text;
                message.Time_send = DateTime.Now.ToLocalTime();
                message.Id_from = user.Id_user;
                message.Id_to = (UsersList.SelectedItem as dll_tcp_chat.User_dll).Id_user;
                if (attachment != null)
                {
                    message.Attachment = attachment;
                }
                else
                {
                    message.Attachment = null;
                }
                try
                {
                    await servise.SendMessage(message);
                    MessageBox.Show("Сообщение отправлено!");
                    messages.Add(message);
                    using (FileStream createStream = new FileStream(message_file_name, FileMode.OpenOrCreate))
                    {
                        byte[] jsonUtf8Bytes = serialize.GetBytesFromList(messages);
                        await JsonSerializer.SerializeAsync<byte[]>(createStream, jsonUtf8Bytes);
                    }
                    MessagePanel.Items.Add(NewMessagePanel(message, user.Name, false));
                }
                catch
                {
                    MessageBox.Show("Ошибка сообщения!");
                }
            }
            attachment = null;
            MessageText.Text = "";
            AttachLabel.Content = "";
        }

        private void AttachmentButton_Click(object sender, RoutedEventArgs e)//сохранение вложения сообщения
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();//диалоговое окно
            dialog.Filter = "All Documents|*.*";

            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                FileInfo f = new FileInfo(dialog.FileName);
                string path = f.FullName;
                attachment = new dll_tcp_chat.Attachment_dll()
                {
                    Body = System.IO.File.ReadAllBytes(path),
                    FileName = System.IO.Path.GetFileName(path)
                };
                Dispatcher.Invoke(new Action(() => AttachLabel.Content = System.IO.Path.GetFileName(path)));
            }
        }

        private void MouseDown_Messages(object sender, MouseButtonEventArgs e)
        {
            Dispatcher.Invoke(new Action(() => (sender as Border).BorderBrush = Brushes.AliceBlue));
        }

        private void SelectionUser(object sender, SelectionChangedEventArgs e)
        {
            Dispatcher.Invoke(new Action(() => UserToText.Text = (UsersList.SelectedItem as dll_tcp_chat.User_dll).Name));
        }
    }
}
