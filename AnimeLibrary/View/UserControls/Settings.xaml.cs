using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Automation;

namespace AnimeLibrary.View.UserControls
{
    public partial class Settings : Window
    {
        public static string? AnimeDirectory { get; set; }
        public static Dictionary<string, string> animeDir = new Dictionary<string, string>();
        public static bool SaveConfig { get; set; }
        public static bool ResetConfig { get; set; }

        public Settings()
        {
            InitializeComponent();
            bool IsValidDirectory(string path)
            {
                return Directory.Exists(path) && Directory.GetFiles(path).Length > 0;
            }

            string config = File.ReadAllText("config.json");
            using JsonDocument jsonDoc = JsonDocument.Parse(config);

            string directory = jsonDoc.RootElement.GetProperty("directory").GetString();

            if (IsValidDirectory(directory))
            {
                AnimeDirectory = directory;
            }
            txtDirectory.Text = AnimeDirectory;
        }

        private void btnCloseS_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            
            var mainWindow = Application.Current.MainWindow as MainWindow;

            if (mainWindow != null)
            {
                _ = mainWindow.LoadCards(animeDir.Keys.ToArray());
                mainWindow.cardScroll.ScrollToTop();
            }
            if (SaveConfig)
            {
                string config = File.ReadAllText("config.json");
                JsonNode jsonNode = JsonNode.Parse(config);
                jsonNode["directory"] = AnimeDirectory;

                File.WriteAllText("config.json", jsonNode.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
            }
            if (ResetConfig)
            {
                string config = File.ReadAllText("config.json");
                JsonNode jsonNode = JsonNode.Parse(config);
                jsonNode["directory"] = "";

                File.WriteAllText("config.json", jsonNode.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
            }

            Close();
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog openFolderDialog = new OpenFolderDialog();
            if (openFolderDialog.ShowDialog() == true)
            {
                txtDirectory.Text = openFolderDialog.FolderName;
                AnimeDirectory = openFolderDialog.FolderName;
                OpenDirectory(AnimeDirectory);
            }
        }

        public void OpenDirectory(string path)
        {
            string idPattern = @"\((?<value>\d+)\)";
            string idPattern2 = @"-\s*(?<value>\d+)";
            string[] subFolderPaths = Directory.GetDirectories(path);
            foreach (string subFolderPath in subFolderPaths)
            {
                string folderName = Path.GetFileName(subFolderPath);
                if (Regex.IsMatch(folderName, idPattern))
                {
                    string id = Regex.Match(folderName, idPattern).Groups["value"].Value;
                    if (!animeDir.ContainsKey(id))
                    {
                        animeDir.Add(id, subFolderPath);
                    }
                }
                else if (Regex.IsMatch(folderName, idPattern2))
                {
                    string id = Regex.Match(folderName, idPattern2).Groups["value"].Value;
                    if (!animeDir.ContainsKey(id))
                    {
                        animeDir.Add(id, subFolderPath);
                    }
                }

            }
        }

        private void chkSaveConfig_Checked(object sender, RoutedEventArgs e)
        {
            SaveConfig = true;
        }

        private void chkResetConfig_Checked(object sender, RoutedEventArgs e)
        {
            ResetConfig = true;
        }
    }
}
