using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AnimeLibrary.View.UserControls
{
    public partial class Settings : Window
    {
        public static string? AnimeDirectory { get; set; }
        public static string? Anime4kPreset { get; set; }
        public static bool Anime4kChanged { get; set; } = false;
        public static bool EnglishTitle { get; set; } = false;
        public static bool EnglishTitleChanged { get; set; } = false;
        public static bool DiscordRPC { get; set; } = false;
        public static bool DiscordRPCChanged { get; set; } = false;

        private RadioButton? selectedPresetRadioButton = null;
        private RadioButton? selectedDirRadioButton = null;

        public static Dictionary<string, string> animeDir = new Dictionary<string, string>();
        public static bool SaveConfig { get; set; }
        public static bool ResetConfig { get; set; }

        public MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
        public Settings()
        {
            InitializeComponent();
            bool IsValidDirectory(string path)
            {
                return Directory.Exists(path) && Directory.GetFiles(path).Length > 0;
            }
            DiscordPresence.UpdatePresence("Managing Settings", "", "icon", "Anime Library", "settings", null, null);
            string config = File.ReadAllText("config.json");
            using JsonDocument jsonDoc = JsonDocument.Parse(config);

            string directory = jsonDoc.RootElement.GetProperty("directory").GetString();
            string currentUpscale = jsonDoc.RootElement.GetProperty("currentUpscale").GetString();

            if (DiscordRPCChanged)
            {
                chkDiscord.IsChecked = DiscordRPC;
            }
            else
            {
                DiscordRPC = jsonDoc.RootElement.GetProperty("discordRPC").GetBoolean();
                chkDiscord.IsChecked = DiscordRPC;
            }

            if (EnglishTitleChanged)
            {
                chkEnglishTitle.IsChecked = EnglishTitle;
                chkEnglishTitle.Checked += chkEnglishTitle_Checked;
                chkEnglishTitle.Unchecked += chkEnglishTitle_Unchecked;
            }
            else
            {
                EnglishTitle = jsonDoc.RootElement.GetProperty("englishTitle").GetBoolean();
                chkEnglishTitle.IsChecked = EnglishTitle;
                chkEnglishTitle.Checked += chkEnglishTitle_Checked;
                chkEnglishTitle.Unchecked += chkEnglishTitle_Unchecked;
            }

            if (IsValidDirectory(directory))
            {
                AnimeDirectory = directory;
            }

            if (!string.IsNullOrEmpty(currentUpscale))
            {
                if (Anime4kChanged == false)
                {
                    Anime4kPreset = currentUpscale;
                }
                
            }

            if (!string.IsNullOrEmpty(Anime4kPreset))
            {
                var radioButtons = new[] { chkA, chkB, chkC, chkAA, chkBB, chkCA };
                var matchedButton = radioButtons.FirstOrDefault(rb => rb.Tag.ToString() == Anime4kPreset);

                if (matchedButton != null)
                {
                    matchedButton.IsChecked = true;
                    selectedPresetRadioButton = matchedButton;
                    Anime4kChanged = true;
                }

                upscaleChks.Visibility = Visibility.Visible;
                blockUpscale.Visibility = Visibility.Visible;
                chkA.Foreground = Brushes.Gray;
                chkB.Foreground = Brushes.Gray;
                chkC.Foreground = Brushes.Gray;
                chkAA.Foreground = Brushes.Gray;
                chkBB.Foreground = Brushes.Gray;
                chkCA.Foreground = Brushes.Gray;
            }
            txtDirectory.Text = AnimeDirectory;
        }

        private void btnCloseS_Click(object sender, RoutedEventArgs e)
        {
            DiscordPresence.UpdatePresence("Browsing Anime Library", "", "icon", "Anime Library", "", null, null);
            Close();
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            DiscordPresence.UpdatePresence("Browsing Anime Library", "", "icon", "Anime Library", "", null, null);

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
                jsonNode["discordRPC"] = DiscordRPC;
                jsonNode["englishTitle"] = EnglishTitle;

                File.WriteAllText("config.json", jsonNode.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
            }
            if (ResetConfig)
            {
                string config = File.ReadAllText("config.json");
                JsonNode jsonNode = JsonNode.Parse(config);
                jsonNode["directory"] = "";
                jsonNode["currentUpscale"] = "";
                jsonNode["discordRPC"] = false;
                jsonNode["englishTitle"] = false;

                File.WriteAllText("config.json", jsonNode.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
            }

            if (DiscordRPC)
            {
                DiscordPresence.InitializeDiscord();
            } else
            {
                DiscordPresence.ClearDiscord();
            }

            if (EnglishTitleChanged)
            {
                mainWindow.AnimeCards.Children.Clear();
                _ = mainWindow.LoadCards(animeDir.Keys.ToArray());
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
                    Anime4kChanged = true;
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

        private void chkUpscale_Click(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb)
            {
                if (rb.IsChecked == true)
                {
                    rb.IsChecked = false;
                }
                else
                {
                    rb.IsChecked = true;
                }
            }
        }

        private void chkUpscale_Checked(object sender, RoutedEventArgs e)
        {
            upscaleChks.Visibility = Visibility.Visible;
            blockUpscale.Visibility = Visibility.Collapsed;
            chkA.Foreground = Brushes.White;
            chkB.Foreground = Brushes.White;
            chkC.Foreground = Brushes.White;
            chkAA.Foreground = Brushes.White;
            chkBB.Foreground = Brushes.White;
            chkCA.Foreground = Brushes.White;
        }

        private void chkUpscale_Unchecked(object sender, RoutedEventArgs e)
        {
            if (Anime4kPreset == null)
            {
                upscaleChks.Visibility = Visibility.Collapsed;
            } else
            {
                blockUpscale.Visibility = Visibility.Visible;
                chkA.Foreground = Brushes.Gray;
                chkB.Foreground = Brushes.Gray;
                chkC.Foreground = Brushes.Gray;
                chkAA.Foreground = Brushes.Gray;
                chkBB.Foreground = Brushes.Gray;
                chkCA.Foreground = Brushes.Gray;
            }
        }

        private void chkEnglishTitle_Checked(object sender, RoutedEventArgs e)
        {
            EnglishTitle = true;
            EnglishTitleChanged = true;
        }

        private void chkEnglishTitle_Unchecked(object sender, RoutedEventArgs e)
        {
            EnglishTitle = false;
            EnglishTitleChanged = true;
        }

        private void chkDiscord_Unchecked(object sender, RoutedEventArgs e)
        {
            DiscordRPC = false;
            DiscordRPCChanged = true;
        }

        private void chkDiscord_Checked(object sender, RoutedEventArgs e)
        {
            DiscordRPC = true;
            DiscordRPCChanged = true;
        }
    }
}
