using DiscordRPC;

namespace AnimeLibrary.View.UserControls
{
    public class DiscordPresence
    {
        public static DiscordRpcClient discordClient;
        public static void InitializeDiscord()
        {
            discordClient = new DiscordRpcClient("1487600457667837982");

            discordClient.OnReady += (sender, e) =>
            {
                UpdatePresence("Browsing Anime Library", "", "icon", "Anime Library", "");
            };


            discordClient.Initialize();
        }

        public static void ClearDiscord()
        {
            if (discordClient != null)
            {
                discordClient.ClearPresence();
                discordClient.Dispose();
            }
        }

        public static void UpdatePresence(string details, string state, string largeImageKey, string largeImageText, string smallImageKey)
        {
            if (discordClient == null) return;
            discordClient.ClearPresence();

            discordClient.SetPresence(new RichPresence()
            {
                Details = details,
                State = state,
                Type = ActivityType.Watching,
                Assets = new Assets()
                {
                    LargeImageKey = largeImageKey,
                    LargeImageText = largeImageText,
                    SmallImageKey = smallImageKey
                },
            });
        }
    }
}
