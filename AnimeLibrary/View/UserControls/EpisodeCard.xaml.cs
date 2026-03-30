using System.Collections;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
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
            string ipcPipe = $@"\\.\pipe\mpv-{Guid.NewGuid()}";
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
                startInfo.Arguments = $"\"{AnimePath}\" --input-ipc-server=\"{ipcPipe}\" --glsl-shaders=\"{GetShaderArg(Settings.Anime4kPreset)}\" --osd-playing-msg=\"{selectedPreset}\"";
            }
            // If no preset is selected, just launch the video without shaders
            else
            {
                startInfo.Arguments = $"\"{AnimePath}\" --input-ipc-server=\"{ipcPipe}\"";
            }

            try
            {
                Process process = Process.Start(startInfo);
                process.EnableRaisingEvents = true;

                _ = Task.Run(async () =>
                {
                    await Task.Delay(2000);

                    while (!process.HasExited)
                    {
                        try
                        {
                            using NamedPipeClientStream pipe = new NamedPipeClientStream(
                                ".",
                                ipcPipe.Replace(@"\\.\pipe\", ""),
                                PipeDirection.InOut);

                            await pipe.ConnectAsync(2000);

                            using StreamWriter writer = new StreamWriter(pipe) { AutoFlush = true };
                            using StreamReader reader = new StreamReader(pipe);

                            // check paused
                            await writer.WriteLineAsync("{\"command\": [\"get_property\", \"pause\"]}");
                            string? pauseResponse = await reader.ReadLineAsync();
                            bool isPaused = false;
                            if (!string.IsNullOrEmpty(pauseResponse))
                            {
                                using JsonDocument doc = JsonDocument.Parse(pauseResponse);
                                if (doc.RootElement.TryGetProperty("data", out JsonElement pauseData))
                                    isPaused = pauseData.GetBoolean();
                            }

                            // check current time
                            await writer.WriteLineAsync("{\"command\": [\"get_property\", \"playback-time\"]}");
                            string? pbResponse = await reader.ReadLineAsync();

                            double playbackSeconds = 0;
                            if (!string.IsNullOrEmpty(pbResponse))
                            {
                                using JsonDocument doc = JsonDocument.Parse(pbResponse);

                                if (doc.RootElement.TryGetProperty("data", out JsonElement dataElement) &&
                                    dataElement.ValueKind == JsonValueKind.Number)
                                {
                                    playbackSeconds = dataElement.GetDouble();
                                }
                            }

                            // check duration
                            await writer.WriteLineAsync("{\"command\": [\"get_property\", \"duration\"]}");
                            string? durResponse = await reader.ReadLineAsync();

                            double durationSeconds = 0;
                            if (!string.IsNullOrEmpty(durResponse))
                            {
                                using JsonDocument doc = JsonDocument.Parse(durResponse);

                                if (doc.RootElement.TryGetProperty("data", out JsonElement dataElement) &&
                                    dataElement.ValueKind == JsonValueKind.Number)
                                {
                                    durationSeconds = dataElement.GetDouble();
                                }
                            }

                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                if (isPaused)
                                {
                                    DiscordPresence.UpdatePresence($"Paused {Title}", $"Episode {AnimeEp}", $"{Image}", $"Paused {Title} - Episode {AnimeEp}", "", null, null);
                                } else
                                {
                                    DateTime now = DateTime.UtcNow;
                                    DateTime start = now.AddSeconds(-playbackSeconds);
                                    double remainingSeconds = durationSeconds - playbackSeconds;
                                    DateTime end = now.AddSeconds(remainingSeconds);
                                    DiscordPresence.UpdatePresence($"Watching {Title}", $"Episode {AnimeEp}", $"{Image}", $"Watching {Title} - Episode {AnimeEp}", "", start, end);
                                }
                                
                            });
                        }
                        catch
                        {
                        }

                        await Task.Delay(2000);
                    }
                });

                process.Exited += (s, args) =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        DiscordPresence.UpdatePresence("Browsing Anime Library", "", "icon", "Anime Library", "", null, null);
                    });
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to launch video player: {ex.Message}");
            }
        }
    }
}
