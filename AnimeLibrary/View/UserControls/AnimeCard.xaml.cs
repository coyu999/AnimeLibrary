using HtmlAgilityPack;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;


namespace AnimeLibrary.View.UserControls
{
    public partial class AnimeCard : UserControl
    {
        public string? AnimeId { get; private set; }
        public static event Action<AnimeCard>? AnyCardClicked;
        public AnimeCard(string? id)
        {
            InitializeComponent();
            this.AnimeId = id;
            AnyCardClicked += OnAnyCardClicked;
            SearchTextBox.OnSearchChanged += HandleSearch;

        }

        public async Task<bool> InitaliseAsync()
        {
            if (string.IsNullOrEmpty(AnimeId)) return false;

            bool success = await GetAnimeScrape(AnimeId);

            if (success)
            {
                getEpisodes(AnimeId);
            }

            return success;
        }

        private void getEpisodes(string thisId)
        {
            string[] extensions = { ".mp4", ".mkv", ".avi", ".mov" };
            Settings.animeDir.TryGetValue(thisId!, out string? animePath);
            string epPattern = @"(?:\s-\s|(?<=\s))(\d+)(?=\s|\[)";
            if (animePath != null)
            {
                var files = Directory.GetFiles(animePath).Where(file => extensions.Contains(Path.GetExtension(file).ToLower())).ToList();

                foreach (string file in files)
                {
                    string fileName = Path.GetFileName(file);
                    if (Regex.IsMatch(fileName, epPattern))
                    {
                        string epNum = Regex.Match(file, epPattern).Groups[1].Value;
                        this.EpisodeCards.Children.Add(new EpisodeCard(epNum, file));
                    }
                }
            }
        }

        private void OnAnyCardClicked(AnimeCard card)
        {
            if (card == this)
            {
                if (this.cardDim.Visibility == Visibility.Collapsed)
                {
                    if (mainBorder.Effect is DropShadowEffect shadowEffect)
                    {
                        shadowEffect.BlurRadius = 500;
                    }
                    this.cardDim.Visibility = Visibility.Visible;
                    this.EpisodeScroll.Visibility = Visibility.Visible;
                    this.SynopsisBox.Visibility = Visibility.Collapsed;
                    this.SynopsisOpen.Visibility = Visibility.Visible;
                    this.SynopsisClose.Visibility = Visibility.Collapsed;
                } else
                {
                    if (mainBorder.Effect is DropShadowEffect shadowEffect)
                    {
                        shadowEffect.BlurRadius = 10;
                    }
                    this.cardDim.Visibility = Visibility.Collapsed;
                    this.EpisodeScroll.Visibility = Visibility.Collapsed;
                    this.SynopsisBox.Visibility = Visibility.Collapsed;
                    this.SynopsisOpen.Visibility = Visibility.Collapsed;
                    this.SynopsisClose.Visibility = Visibility.Collapsed;
                }
            } 
            else
            {
                if (mainBorder.Effect is DropShadowEffect shadowEffect)
                {
                    shadowEffect.BlurRadius = 10;
                }
                this.cardDim.Visibility = Visibility.Collapsed;
                this.EpisodeScroll.Visibility = Visibility.Collapsed;
                this.SynopsisBox.Visibility = Visibility.Collapsed;
                this.SynopsisOpen.Visibility = Visibility.Collapsed;
                this.SynopsisClose.Visibility = Visibility.Collapsed;
            }
        }

        private async Task<bool> GetAnimeScrape(string id)
        {
            try
            {
                using HttpClient client = new();
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
                string html = await client.GetStringAsync($"https://myanimelist.net/anime/{id}");

                HtmlDocument doc = new();
                doc.LoadHtml(html);
                var titleNode = doc.DocumentNode.SelectSingleNode("//h1[contains(@class, 'title-name')]/strong");
                var airedNode = doc.DocumentNode.SelectSingleNode("//span[text()='Aired:']/following-sibling::text()");
                var studioNode = doc.DocumentNode.SelectSingleNode("//span[text()='Studios:']/following-sibling::a");
                var scoreNode = doc.DocumentNode.SelectSingleNode("//span[@itemprop='ratingValue']");
                var imageNode = doc.DocumentNode.SelectSingleNode("//img[contains(@class, 'lazyload')]");
                var synopsisNode = doc.DocumentNode.SelectSingleNode("//p[@itemprop='description']");

                if (titleNode == null) return false;

                if (titleNode != null)
                {
                    string title = titleNode.InnerText.Trim();
                    animeTitle.Text = title;
                }

                if (airedNode != null)
                {
                    string aired = airedNode.InnerText.Trim();
                    animeAired.Text = aired;
                }

                if (studioNode != null) 
                {
                    string studio = studioNode.InnerText.Trim();
                    animeStudio.Text = studio;
                }

                if (scoreNode != null)
                {
                    string score = scoreNode.InnerText.Trim();

                    if (double.TryParse(score, out double scoreValue))
                    {
                        animeScore.Text = scoreValue.ToString("0.00");
                    }
                    else 
                    { 
                        animeScore.Text = "N/A"; 
                    }
                }

                if (imageNode != null)
                {
                    string imageUrl = imageNode.GetAttributeValue("data-src", "");
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        BitmapImage bitmap = new();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(imageUrl, UriKind.Absolute);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        
                        animeImage.Source = bitmap;
                    }
                }

                if (synopsisNode != null)
                {
                    animeSynopsis.Text = WebUtility.HtmlDecode(synopsisNode.InnerText.Trim());
                }

                return true;

            } 
            catch (Exception ex) {
                Console.WriteLine($"Error loading card: {ex.Message}");
                return false;
            }
            
        }

        private void Border_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            AnyCardClicked?.Invoke(this);
        }

        private void HandleSearch(string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
            {
                this.Visibility = Visibility.Visible;
                return;
            }
            bool isMatch = animeTitle.Text.Contains(searchText, StringComparison.OrdinalIgnoreCase);
            this.Visibility = isMatch ? Visibility.Visible : Visibility.Collapsed;
        }

        private void SynopsisBtn_Click(object sender, RoutedEventArgs e)
        {
            this.SynopsisBox.Visibility = Visibility.Visible;
            this.SynopsisOpen.Visibility = Visibility.Collapsed;
            this.SynopsisClose.Visibility = Visibility.Visible;
        }

        private void SynopsisBtnC_Click(object sender, RoutedEventArgs e)
        {
            this.SynopsisBox.Visibility = Visibility.Collapsed;
            this.SynopsisClose.Visibility = Visibility.Collapsed;
            this.SynopsisOpen.Visibility = Visibility.Visible;
        }
    }
}
