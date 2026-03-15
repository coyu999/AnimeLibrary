using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;

namespace AnimeLibrary.View.UserControls
{
    public partial class Settings : Window
    {
        public static string? AnimeDirectory { get; set; }
        public static Dictionary<string, string> animeDir = new Dictionary<string, string>();

        public Settings()
        {
            InitializeComponent();
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
            }
            Close();
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog openFolderDialog = new OpenFolderDialog();
            string idPattern = @"\((?<value>\d+)\)";
            if (openFolderDialog.ShowDialog() == true)
            {
                txtDirectory.Text = openFolderDialog.FolderName;
                AnimeDirectory = openFolderDialog.FolderName;
                string[] subFolderPaths = Directory.GetDirectories(openFolderDialog.FolderName);
                foreach (string subFolderPath in subFolderPaths)
                {
                    string folderName = Path.GetFileName(subFolderPath);
                    if (Regex.IsMatch(folderName, idPattern))
                    {
                        string id = Regex.Match(folderName, idPattern).Groups["value"].Value;
                        if (!animeDir.ContainsKey(folderName))
                        {
                            animeDir.Add(id, subFolderPath);
                        }
                    }
                }
            }
        }
    }
}
