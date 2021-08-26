using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace VRCAuthChecker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public string filePath = "null";

        public string[] accounts;

        public string folderName = "null";

        public Uri authUri = new Uri("https://vrchat.com/api/1/auth");

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            Debugger.Text = "================Start Of Debugging================\n";
            Log("Cleared Logs\n");
        }

        private void CheckButton_Click(object sender, RoutedEventArgs e)
        {

            if (accounts == null)
            {
                Log("ERROR: No given file\n");
                return;
            }


            Log($"=========Checking Accounts=========\n");

            folderName = DateTime.Now.ToString("dd-MM-yyyy HH-mm-ss");

            if (!Directory.Exists("Output"))
            {
                Directory.CreateDirectory("Output");
            }

            Directory.CreateDirectory($"Output\\{folderName}");

            CheckAccounts();

        }

        private void OnDelayChanged(object sender, TextChangedEventArgs e)
        {
            int parsed;
            if (!int.TryParse(delayInput.Text, out parsed))
            {
                Log("Please input a valid delay time");
                delayInput.Text = "5000";
            }
        }

        private async void CheckAccounts()
        {
            var passAccounts = new List<string>();
            var failAccounts = new List<string>();

            HttpClient request = new HttpClient();
            request.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.164 Safari/537.36 OPR/77.0.4054.298");


            for (int i = 0; i < accounts.Length; ++i)
            {
                Log($"Checking {i + 1}: {accounts[i]}...");

                var rM = new HttpRequestMessage(HttpMethod.Get, authUri);

                rM.Headers.Add("Cookie", $"auth={accounts[i]}");


                try
                {
                    var webResponse = await request.SendAsync(rM);
                    var response = await webResponse.Content.ReadAsStringAsync();
                    if (response.Contains("true"))
                    {
                        passAccounts.Add(accounts[i]);
                        Log($"{accounts[i]} - PASS\n");
                    }
                    
                    if(response.ToLower().Contains("missing credentials"))
                    {
                        failAccounts.Add(accounts[i]);
                        Log($"{accounts[i]} - FAIL\n");
                    }

                }
                catch (Exception ex)
                {
                    if (ex.Message.ToLower().Contains("unauthorized"))
                    {
                        failAccounts.Add(accounts[i]);
                        Log($"{accounts[i]} - FAIL\n");
                    }
                    else
                    {
                        Log($"UNKNOWN ERROR, CHECK IMMEDIATELY");
                        Log(ex.ToString());
                    }
                }
                await Task.Delay(int.Parse(delayInput.Text));
            }

            await File.WriteAllLinesAsync($"Output\\{folderName}\\PASS.txt", passAccounts);
            await File.WriteAllLinesAsync($"Output\\{folderName}\\FAIL.txt", failAccounts);
        }

        private void FileButton_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog()
            {
                Multiselect = false,
                Filter = "Text|*.txt"
            };
            var dialogResult = fileDialog.ShowDialog();
            if(dialogResult == true)
            {
                filePath = fileDialog.FileName;
            }

            accounts = File.ReadAllLines(filePath);

            Log($"New file path added: {filePath}");
            Log($"READY TO CHECK ACCOUNTS");
        }
        
        private void FileButton_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            var paths = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (paths.Length > 1)
            {
                Log($"ERROR: Please select only ONE file");
                return;
            }

            var path = paths[0];

            if (!path.Contains(".txt"))
            {
                Log($"ERROR: Please select a .txt file");
                return;
            }

            accounts = File.ReadAllLines(path);

            filePath = path;
            Log($"New file path added: {filePath}");
            Log($"READY TO CHECK ACCOUNTS");
        }

        private void Log(string text)
        {
            Debugger.Text += $"\n[{DateTime.Now.ToString("HH:mm:ss")}] {text}";
            Debugger.ScrollToEnd();
        }
    }
}
