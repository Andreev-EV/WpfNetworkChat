using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Net.Sockets;
using System.Net;
using System.Xml.Linq;
using System.Threading;
using System.Reflection.PortableExecutable;

namespace WpfNetworkChat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string host = "127.0.0.1";
        int port = 8888;
        TcpClient client = new TcpClient();
        StreamReader? Reader = null;
        StreamWriter? Writer = null;
        bool IsbtnClicked = false;

    public MainWindow()
        {
            InitializeComponent();
        }

      

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            if (client != null && client.Connected)
            {
                // Отправка сообщения на сервер
                string message = tbMessage.Text;
                Writer.WriteLine(message);
                Writer.Flush();
                AddMessage($"Ты: {message}");
                tbMessage.Clear();
            }
            else
            {
                AddMessage("Not connected to server.");
            }
        }
      

        private async void btnConnDisconn_Click(object sender, RoutedEventArgs e)
        {
            if (client == null || !client.Connected)
            {
                if(tbUserName.Text != "" && tbUserName.Text != "Имя пользователя")
                {
                    try
                    {
                        client?.Connect(host, port); //подключение клиента
                        Reader = new StreamReader(client.GetStream());
                        Writer = new StreamWriter(client.GetStream());
                        if (Writer is null || Reader is null) return;
                        // запускаем новый поток для получения данных
                        Task.Run(() => ReceiveMessageAsync(Reader));
                        AddMessage("Вы присоеденились к чату");
                        Writer.WriteLine(tbUserName.Text);
                        Writer.Flush();
                        MainWin.Title = $"Чат TcpClient : {tbUserName.Text}";
                    }
                    catch (Exception ex)
                    {
                        AddMessage($"Error: {ex.Message}");
                    }
                }
                else
                {
                    MessageBox.Show("Укажите имя пользователя!");
                    tbUserName.Text = "Имя пользователя";
                }
                
            }
            //else
            //{
            //    Disconnect();
            //}
           
        }

        private void Disconnect()
        {
            Reader?.Close();
            Writer?.Close();
            client.Close();
        }

        private void AddMessage(string message)
        {
            lbChat.Items.Add($"{message}");
        }
        // получение сообщений
        async Task ReceiveMessageAsync(StreamReader reader)
        {
            while (true)
            {
                try
                {
                    // считываем ответ в виде строки
                    string? message = await reader.ReadLineAsync();
                    // если пустой ответ, ничего не выводим на консоль
                    if (string.IsNullOrEmpty(message)) continue;
                    //вывод сообщения
                    Dispatcher.Invoke(() =>
                    {
                        AddMessage($"{message}");
                    });
                }
                catch
                {
                    break;
                }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Disconnect();
            Writer.WriteLine(tbUserName.Text);
            Writer.Flush();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Disconnect();
        }
    }
}
