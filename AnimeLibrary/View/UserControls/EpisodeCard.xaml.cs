using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace AnimeLibrary.View.UserControls
{
    /// <summary>
    /// Interaction logic for EpisodeCard.xaml
    /// </summary>
    public partial class EpisodeCard : UserControl
    {
        public string? AnimePath { get; private set; }
        public string? AnimeEp { get; private set; }
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
            if (string.IsNullOrEmpty(AnimePath)) return;

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = mpvPath,
                Arguments = $"\"{AnimePath}\"",
                UseShellExecute = false
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
