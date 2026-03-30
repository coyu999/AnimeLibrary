using DiscordRPC;
using System.Collections;
using System.Diagnostics;

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
                UpdatePresence("Browsing Anime Library", "", "icon", "Anime Library", "", null, null);
            };


            discordClient.Initialize();
        }

        public static void ClearDiscord()
        {
            if (discordClient != null)
            {
                discordClient.ClearPresence();
                discordClient.Dispose();
                discordClient = null;
            }
        }

        public static void UpdatePresence(string details, string state, string largeImageKey, string largeImageText, string smallImageKey, DateTime? startTime, DateTime? endTime)
        {
            if (discordClient == null) return;

            var presence = new RichPresence()
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
            };

            if (startTime.HasValue && endTime.HasValue)
            {
                presence.Timestamps = new Timestamps()
                {
                    Start = startTime,
                    End = endTime
                };
            }

            discordClient.SetPresence(presence);
        }
    }
}
