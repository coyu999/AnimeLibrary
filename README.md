# Anime Library
A WPF Application designed to help you organize and enjoy your local anime collection by automating metadata retrieval and providing a modern viewing interface.

---

## 📦 Requirements

- Windows OS
- .NET 6 or later
- Visual Studio 2022 or later (for building from source)

---

## ✏️ Usage

1. Click the 🛠 **Settings** icon in the top-left to configure your library.
2. Click **Browse** to select the root folder containing your anime.
3. **Important:** Ensure your folders are named containing the MyAnimeList ID. e.g. "*anything* (12345)" or "*anything* - 12345"
4. Click ✔️ **Confirm** to load the library; the app will automatically scrape titles, art, and ratings.
5. Click an **Anime Card** to view available episodes.
6. Click an **Episode Button** to launch the video directly in **mpvnet**.

---

## 🖼 Features & Scraped Data

- **Automatic Metadata:** Fetches Title, Aired Date, Studio, and Score via MyAnimeList scraping.
- **Dynamic UI:** Responsive grid layout using custom-styled AnimeCards.
- **Media Integration:** Seamless hand-off to mpvnet for high-quality playback.
- **Visual Effects:** Modern dark-themed gradients and interactive "window dimming" for modals.

---

## 🛠 Dependencies

- HtmlAgilityPack – For parsing MyAnimeList metadata.
- System.Net.Http – For handling web requests.
- mpvnet – Required for video playback
