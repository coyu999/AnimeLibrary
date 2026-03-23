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
        public EpisodeCard(string num, string filePath)
        {
            InitializeComponent();
            episodeNum.Content = num;
            this.AnimeEp = num;
            this.AnimePath = filePath;
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

            if (!string.IsNullOrEmpty(Settings.Anime4kPreset))
            {
                var selectedPreset = anime4kOSD.GetValueOrDefault(Settings.Anime4kPreset);
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = mpvPath,
                    Arguments = $"\"{AnimePath}\" --glsl-shaders=\"{GetShaderArg(Settings.Anime4kPreset)}\" --osd-playing-msg=\"{selectedPreset}\"",
                    UseShellExecute = false,
                };

                try
                {
                    Process.Start(startInfo);

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error launching media player: {ex.Message}");
                }
            } else
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = mpvPath,
                    Arguments = $"\"{AnimePath}\"",
                    UseShellExecute = false,
                };

                try
                {
                    Process.Start(startInfo);

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error launching media player: {ex.Message}");
                }
            }
        }
    }
}
