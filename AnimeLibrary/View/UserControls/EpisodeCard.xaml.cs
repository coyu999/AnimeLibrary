using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace AnimeLibrary.View.UserControls
{
    public partial class EpisodeCard : UserControl
    {
        public string? AnimePath { get; private set; }
        public string? Title { get; private set; }
        public string? Studio { get; private set; }
        public string? Score { get; private set; }
        public string? Image { get; private set; }

        public string? AnimeEp { get; private set; }
        public static readonly Dictionary<string, string> anime4kOSD = new Dictionary<string, string>
        {
            {"A", "Anime4K: Mode A (HQ)"},
            {"B", "Anime4K: Mode B (HQ)"},
            {"C", "Anime4K: Mode C (HQ)"},
            {"AA", "Anime4K: Mode A+A (HQ)"},
            {"BB", "Anime4K: Mode B+B (HQ)"},
            {"CA", "Anime4K: Mode C+A (HQ)"}
        };
        public EpisodeCard(string num, string filePath, string? title, string? studio, string? score, string? image)
        {
            InitializeComponent();
            episodeNum.Content = num;
            this.AnimeEp = num;
            this.AnimePath = filePath;
            this.Title = title;
            this.Studio = studio;
            this.Score = score;
            this.Image = image;
        }

        private void episodeNum_Click(object sender, RoutedEventArgs e)
        {
            string mpvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mpv", "mpvnet.exe");
            string shadersPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mpv", "shaders");
            string config = File.ReadAllText("config.json");
            using JsonDocument jsonDoc = JsonDocument.Parse(config);
            if (string.IsNullOrEmpty(AnimePath)) return;

            string GetShaderArg(string preset) => string.Join(";", jsonDoc.RootElement
                    .GetProperty($"{preset}")
                    .EnumerateArray()
                    .Select(element => Path.Combine(shadersPath, element.GetString())));

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = mpvPath,
                UseShellExecute = false,
            };

            // If a preset is selected, launch the video with the corresponding shaders and OSD message
            if (!string.IsNullOrEmpty(Settings.Anime4kPreset))
            {
                var selectedPreset = anime4kOSD.GetValueOrDefault(Settings.Anime4kPreset);
                startInfo.Arguments = $"\"{AnimePath}\" --glsl-shaders=\"{GetShaderArg(Settings.Anime4kPreset)}\" --osd-playing-msg=\"{selectedPreset}\"";
            }
            // If no preset is selected, just launch the video without shaders
            else
            {
                startInfo.Arguments = $"\"{AnimePath}\"";
            }

            try
            {
                Process process = Process.Start(startInfo);
                process.EnableRaisingEvents = true;

                process.Exited += (s, args) =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        DiscordPresence.UpdatePresence("Browsing Anime Library", "", "icon", "Anime Library", "");
                    });
                };

                DiscordPresence.UpdatePresence($"Watching {this.Title}", $"Episode {AnimeEp}", $"{this.Image}", $"Watching {this.Title} - Episode {AnimeEp}", "");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to launch video player: {ex.Message}");
            }
        }
    }
}
