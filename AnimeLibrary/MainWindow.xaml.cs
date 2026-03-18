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
            var existingIds = AnimeCards.Children.OfType<AnimeCard>().Select(c => c.AnimeId).ToHashSet();
            tbSearch.Visibility = Visibility.Visible;
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
                }
            }
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