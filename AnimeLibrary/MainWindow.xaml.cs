using System.Windows;
using AnimeLibrary.View;
using AnimeLibrary.View.UserControls;

namespace AnimeLibrary
{

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

        }

        public async Task LoadCards(string[] ids)
        {
            AnimeCards.Children.Clear();
            foreach (var id in ids)
            {
                bool alreadyExists = AnimeCards.Children.OfType<AnimeCard>().Any(card => card.AnimeId == id);
                if (!alreadyExists)
                {
                    AnimeCards.Children.Add(new AnimeCard(id));
                }
            }
            tbSearch.Visibility = Visibility.Visible;
        }

        private void btnCloseW_Click(object sender, RoutedEventArgs e)
        {
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
    }
}