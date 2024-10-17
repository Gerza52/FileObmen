using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using Microsoft.Win32;

namespace FileSharingApp
{
    public partial class MainWindow : Window
    {
        private List<string> _sentFiles = new List<string>();
        private List<string> _receivedFiles = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
        }

        // ... (остальной код)

        private void SendFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var fileBytes = File.ReadAllBytes(FilePathTextBox.Text);

                using (var client = new TcpClient("192.168.0.100", 12345))
                using (var stream = client.GetStream())
                {
                    int bytesSent = 0;
                    while (bytesSent < fileBytes.Length)
                    {
                        int bytesToSend = Math.Min(fileBytes.Length - bytesSent, 1024);
                        stream.Write(fileBytes, bytesSent, bytesToSend);
                        bytesSent += bytesToSend;
                        FileTransferProgressBar.Value = (double)bytesSent / fileBytes.Length * 100;
                    }

                    _sentFiles.Add(FilePathTextBox.Text);
                    SentFilesListBox.ItemsSource = _sentFiles;
                    MessageBox.Show("File sent successfully!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending file: {ex.Message}");
            }
            finally
            {
                FileTransferProgressBar.Value = 0;
            }
        }

        private void ReceiveFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var listener = new TcpListener(IPAddress.Any, 12345);
                listener.Start();

                using (var client = listener.AcceptTcpClient())
                using (var stream = client.GetStream())
                using (var fileStream = File.Create("received_file.txt"))
                {
                    int bytesReceived = 0;
                    int totalBytes = (int)client.ReceiveBufferSize;

                    byte[] buffer = new byte[1024];
                    int bytesRead = 0;

                    while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        fileStream.Write(buffer, 0, bytesRead);
                        bytesReceived += bytesRead;
                        FileTransferProgressBar.Value = (double)bytesReceived / totalBytes * 100;
                    }

                    _receivedFiles.Add("received_file.txt");
                    ReceivedFilesListBox.ItemsSource = _receivedFiles;
                    MessageBox.Show("File received successfully!");
                }

                listener.Stop();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error receiving file: {ex.Message}");
            }
            finally
            {
                FileTransferProgressBar.Value = 0;
            }
        }
    }
}