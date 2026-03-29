using AnimeLibrary.View.UserControls;
using DiscordRPC;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace AnimeLibrary
{

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            Settings settings = new Settings();
            bool IsValidDirectory(string path)
            {
                return Directory.Exists(path) && Directory.GetFiles(path).Length > 0;
            }

            string config = File.ReadAllText("config.json");
            using JsonDocument jsonDoc = JsonDocument.Parse(config);
            bool discordRPC = jsonDoc.RootElement.GetProperty("discordRPC").GetBoolean();

            if (discordRPC)
            {
                DiscordPresence.InitializeDiscord();
            }

            string directory = jsonDoc.RootElement.GetProperty("directory").GetString();

            if (IsValidDirectory(directory))
            {
                settings.OpenDirectory(directory);
                _ = LoadCards(Settings.animeDir.Keys.ToArray());
            }

            
        }
        public async Task LoadCards(string[] ids)
        {
            var existingIds = AnimeCards.Children.OfType<AnimeCard>().Select(c => c.AnimeId).ToHashSet();
            var newIds = ids.Where(id => !existingIds.Contains(id)).ToList();
            var cardTasks = newIds.Select(async id =>
            {
                var newCard = new AnimeCard(id);
                bool success = await newCard.InitaliseAsync();
                return new { Card = newCard, Success = success };
            }).ToList();

            while (cardTasks.Count > 0)
            {
                var finishedTask = await Task.WhenAny(cardTasks);
                cardTasks.Remove(finishedTask);

                var result = await finishedTask;

                if (result.Success)
                {
                    AnimeCards.Children.Add(result.Card);
                    tbSearch.Visibility = Visibility.Visible;
                    btnClearCards.Visibility = Visibility.Visible;
                }
            }
        }

        private void btnCloseW_Click(object sender, RoutedEventArgs e)
        {
            DiscordPresence.ClearDiscord();
            Application.Current.Shutdown();
        }

        private void btnMaxW_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                WindowState = WindowState.Maximized;
            }
        }

        private void btnMinW_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Grid_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            windowDim.Visibility = Visibility.Visible;
            Settings settings = new Settings();
            settings.Owner = this;
            settings.ShowDialog();
            windowDim.Visibility = Visibility.Collapsed;
        }

        private void btnClearCards_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            TrashHoverInfo.Visibility = Visibility.Visible;
        }

        private void btnClearCards_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            TrashHoverInfo.Visibility = Visibility.Collapsed;
        }

        private void btnClearCards_Click(object sender, RoutedEventArgs e)
        {
            AnimeCards.Children.Clear();
            tbSearch.Visibility = Visibility.Collapsed;
            btnClearCards.Visibility = Visibility.Collapsed;

            string config = File.ReadAllText("config.json");
            JsonNode jsonNode = JsonNode.Parse(config);
            jsonNode["directory"] = "";
            jsonNode["currentUpscale"] = "";
            jsonNode["discordRPC"] = false;
            jsonNode["englishTitle"] = false;

            File.WriteAllText("config.json", jsonNode.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
            Settings.AnimeDirectory = null;
            Settings.Anime4kPreset = null;
            Settings.animeDir.Clear();
        }
    }
}