using HtmlAgilityPack;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;


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
            if (id != null)
            {
                this.Loaded += async (_, _) => await GetAnimeScrape(id);
                this.Loaded += (_, _) => getEpisodes(id);
            }
            SearchTextBox.OnSearchChanged += HandleSearch;

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
                } else
                {
                    if (mainBorder.Effect is DropShadowEffect shadowEffect)
                    {
                        shadowEffect.BlurRadius = 10;
                    }
                    this.cardDim.Visibility = Visibility.Collapsed;
                    this.EpisodeScroll.Visibility = Visibility.Collapsed;
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
            }
        }

        private async Task GetAnimeScrape(string id)
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

            } 
            catch (Exception ex) {
                Console.WriteLine($"Error loading image: {ex.Message}");
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
    }
}
