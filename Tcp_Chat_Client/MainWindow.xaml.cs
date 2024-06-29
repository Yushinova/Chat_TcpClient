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
       // public ObservableCollection<dll_tcp_chat.Message_dll> messages_wpf = new ObservableCollection<dll_tcp_chat.Message_dll>();
        public WPF_servise.Servise servise = new WPF_servise.Servise();
        public dll_tcp_chat.Attachment_dll attachment = new dll_tcp_chat.Attachment_dll();
       // public ObservableCollection<dll_tcp_chat.User_dll> users_dll = new ObservableCollection<dll_tcp_chat.User_dll>();
        public MainWindow()
        {
            InitializeComponent();
           
            AuthorUser();//авторизация по уже сохраненным данным
            
        }
  
        private void AuthorUser()
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
        public StackPanel NewMessagePanel(dll_tcp_chat.Message_dll message, bool isNew)//если новое выделить цветом
        {
            StackPanel panel = new StackPanel();
            panel.Orientation = Orientation.Vertical;
            TextBlock user_from_text = new TextBlock();
            user_from_text.Text = $"[{users.First(u => u.Id_user == message.Id_from).Name} {message.Time_send.ToLongDateString()}]";
            panel.Children.Add(user_from_text);
            TextBlock message_text = new TextBlock();
            message_text.Text = message.Text;
            panel.Children.Add(message_text);
            if (message.Attachment != null)
            {
                StackPanel attach_panel = new StackPanel();
                attach_panel.Orientation = Orientation.Horizontal;
                TextBlock attach_text = new TextBlock();
                attach_text.Text = System.IO.Path.GetExtension(message.Attachment.FileName);//расширение
                attach_panel.Children.Add(attach_text);
                Button button = new Button();
                button.Content = "Add";
                attach_panel.Children.Add(button);//подвесить обработчик и картинку
                panel.Children.Add(attach_panel);
            }
            return panel;
        }
        private async void SetMessages()
        {
            List<dll_tcp_chat.Message_dll> message_from_DB = await servise.GetAllMessage();
            string message_file_name = user.Login + "message.json";
            if(messages.Count>0)
            {
                foreach (var item in message_from_DB)
                {
                    if(!messages.Contains(item))
                    {
                        MessagePanel.Children.Add(NewMessagePanel(item, true));
                    }
                }
            }
            else
            {
                messages = message_from_DB;
                using (FileStream createStream = new FileStream(message_file_name, FileMode.OpenOrCreate))
                {
                    JsonSerializer.Serialize<List<dll_tcp_chat.Message_dll>>(createStream,message_from_DB);
                }
                foreach (var item in message_from_DB)
                {             
                    MessagePanel.Children.Add(NewMessagePanel(item,true));
                }
            }
            //надо проверить 
        }

        private async void RegistrButton_Click(object sender, RoutedEventArgs e)
        {
            user = await servise.RegistrUser(LoginText.Text, PasswordText.Text, NameText.Text);
            users = await servise.GetAllUsers();
            if(users!=null)
            {
                users.Remove(users.First(u => u.Login == user.Login));//убираем из контактов самого себя
                UsersList.ItemsSource = users;
            }    
           if(user!=null)
            {
                RegistrationGrid.Visibility = Visibility.Hidden;
                MainGrid.Visibility = Visibility.Visible;
            }
        }

        private async void AuthorButton_Click(object sender, RoutedEventArgs e)
        {
            string message_file_name = user.Login + "message.json";
            if (AuthorUsers.SelectedIndex!=-1)
            {
                if (System.IO.File.Exists(AuthorUsers.SelectedItem.ToString()))
                {
                    using (StreamReader r = new StreamReader(AuthorUsers.SelectedItem.ToString()))
                    {
                        string json = r.ReadToEnd();
                        user = JsonSerializer.Deserialize<dll_tcp_chat.User_reg_dll>(json);
                    }
                  
                    UserPanel.DataContext = user;
                    users = await servise.GetAllUsers();
                    if(users!=null && users.Count>0)
                    {
                        users.Remove(users.First(u => u.Login == user.Login));//убираем из контактов самого себя
                        UsersList.ItemsSource = users;
                        RegistrationGrid.Visibility = Visibility.Hidden;
                        AuthorPanel.Visibility = Visibility.Hidden;
                        MainGrid.Visibility = Visibility.Visible;
                    }
                    if (System.IO.File.Exists(message_file_name))//записанные сообщения юзера
                    {
                        using (StreamReader r = new StreamReader(message_file_name))
                        {
                            string json = r.ReadToEnd();
                            messages = JsonSerializer.Deserialize<List<dll_tcp_chat.Message_dll>>(json);
                            foreach (var item in messages)
                            {
                                MessagePanel.Children.Add(NewMessagePanel(item, false));
                            }
                        }
                    }
                    //авторизация уже сохраненных юзеров из файликов
                }
            }
           
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            if(UserToText.Text!=string.Empty)
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
                try
                {                 
                    await servise.SendMessage(message);
                    MessageBox.Show("Сообщение отправлено!");
                    messages.Add(message);
                }
                catch
                {
                    MessageBox.Show("Ошибка сообщения!");
                }
            }
            attachment = new dll_tcp_chat.Attachment_dll();
            MessageText.Text = "";
            AttachLabel.Content = "";
        }

        private void SelectionChanged_Users(object sender, SelectionChangedEventArgs e)
        {
            Dispatcher.Invoke(new Action(()=>UserToText.Text=(UsersList.SelectedItem as dll_tcp_chat.User_dll).Name));
        }

        private void AttachmentButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();//диалоговое окно
            dialog.Filter = "All Documents|*.*";

            // Show open file dialog box
            bool? result = dialog.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                FileInfo f = new FileInfo(dialog.FileName);
                string path = f.FullName;
                attachment = new dll_tcp_chat.Attachment_dll()
                { 
                    Body = System.IO.File.ReadAllBytes(path),
                    FileName = System.IO.Path.GetFileName(path)
                } ;
                Dispatcher.Invoke(new Action(() => AttachLabel.Content = System.IO.Path.GetFileName(path)));
            }
        }
    }
}
