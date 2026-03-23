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
using System.Windows.Controls;

namespace AnimeLibrary.View.UserControls
{
    public partial class Settings : Window
    {
        public static string? AnimeDirectory { get; set; }
        public static string? Anime4kPreset { get; set; }

        private RadioButton? selectedPresetRadioButton = null;
        private RadioButton? selectedDirRadioButton = null;

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
            string currentUpscale = jsonDoc.RootElement.GetProperty("currentUpscale").GetString();

            if (IsValidDirectory(directory))
            {
                AnimeDirectory = directory;
            }

            if (!string.IsNullOrEmpty(currentUpscale))
            {
                Anime4kPreset = currentUpscale;
            }

            if (!string.IsNullOrEmpty(Anime4kPreset))
            {
                var radioButtons = new[] { chkA, chkB, chkC, chkAA, chkBB, chkCA };
                var matchedButton = radioButtons.FirstOrDefault(rb => rb.Tag.ToString() == Anime4kPreset);

                if (matchedButton != null)
                {
                    matchedButton.IsChecked = true;
                    selectedPresetRadioButton = matchedButton;
                }
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
                jsonNode["currentUpscale"] = Anime4kPreset;

                File.WriteAllText("config.json", jsonNode.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
            }
            if (ResetConfig)
            {
                string config = File.ReadAllText("config.json");
                JsonNode jsonNode = JsonNode.Parse(config);
                jsonNode["directory"] = "";
                jsonNode["currentUpscale"] = "";

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

        private void ConfigSettings_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb)
            {
                if (rb.Tag.ToString() == "Save")
                {
                    SaveConfig = true;
                    ResetConfig = false;
                }
                else if (rb.Tag.ToString() == "Reset")
                {
                    SaveConfig = false;
                    ResetConfig = true;
                }
            }
        }

        private void chkResetConfig_Checked(object sender, RoutedEventArgs e)
        {
            ResetConfig = true;
        }


        private void Preset_Click(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb)
            {
                if (selectedPresetRadioButton == rb)
                {
                    rb.IsChecked = false;
                    selectedPresetRadioButton = null;
                    Anime4kPreset = null;
                } else
                {
                    selectedPresetRadioButton = rb;
                    Anime4kPreset = rb.Tag.ToString();
                }
            }
        }

        private void Directory_Click(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb)
            {
                if (selectedDirRadioButton == rb)
                {
                    rb.IsChecked = false;
                    selectedDirRadioButton = null;
                }
                else
                {
                    selectedDirRadioButton = rb;
                    if (rb.Tag.ToString() == "Save")
                    {
                        SaveConfig = true;
                        ResetConfig = false;
                    }
                    else if (rb.Tag.ToString() == "Reset")
                    {
                        SaveConfig = false;
                        ResetConfig = true;
                    }
                }
            }
        }
    }
}
