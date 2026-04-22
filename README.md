<div align="center">

# Media Bar Recommendations

### A Jellyfin Plugin

[![License: MIT](https://img.shields.io/badge/license-MIT-blue)](LICENSE)
[![Current Release](https://img.shields.io/github/release/Jycreyn/jellyfin-plugin-media-bar-reco.svg)](https://github.com/Jycreyn/jellyfin-plugin-media-bar-reco/releases)
[![Jellyfin](https://img.shields.io/badge/Jellyfin-10.11-purple)](https://jellyfin.org)

</div>

---

A companion plugin for [IAmParadox27's Media Bar](https://github.com/IAmParadox27/jellyfin-plugin-media-bar) that replaces random content selection with a **personalized recommendation engine**.

Instead of showing random movies, the Media Bar will display films scored and ranked based on your actual watch history — similar to how Netflix curates its featured content.

## How It Works

The plugin runs as a native Jellyfin scheduled task (daily at 3 AM by default). Each run:

1. Fetches all unwatched movies from your library
2. Builds a genre frequency map from your watch history
3. Scores each movie across three weighted factors:
   - **Community rating** — IMDB/TMDB score
   - **Genre affinity** — how closely genres match what you watch most
   - **Recency** — bonus for movies recently added to your library
4. Takes the Top N results and writes them to a Jellyfin playlist
5. The Media Bar plugin reads that playlist as its featured content

No Docker container, no external scripts — everything runs inside Jellyfin.

---

## Installation

### Prerequisites

- Jellyfin `10.11.x`
- [Media Bar plugin](https://github.com/IAmParadox27/jellyfin-plugin-media-bar) already installed and working

### Via Plugin Repository

1. Go to **Dashboard → Plugins → Repositories → +**
2. Add the following URL:
   ```
   https://raw.githubusercontent.com/Jycreyn/jellyfin-plugin-media-bar-reco/main/manifest.json
   ```
3. Go to **Catalogue** and install **Media Bar Recommendations**
4. Restart Jellyfin

### Manual Installation

1. Download the latest `.zip` from the [Releases](https://github.com/Jycreyn/jellyfin-plugin-media-bar-reco/releases) page
2. Extract it into your Jellyfin plugins folder:
   ```
   /config/data/plugins/MediaBarRecommendations_1.0.0.0/
   ```
3. Restart Jellyfin

---

## Configuration

### 1. Configure this plugin

Go to **Dashboard → Plugins → Media Bar Recommendations** and set:

| Setting | Description |
|---|---|
| **User** | The user whose watch history drives the recommendations |
| **Playlist name** | Name of the auto-managed playlist (default: `Featured Recommendations`) |
| **Number of items** | How many movies to include (5–50) |
| **Rating weight** | Importance of the IMDB/TMDB community score |
| **Genre affinity weight** | Importance of matching genres from your history |
| **Recency weight** | Bonus for recently added movies |
| **Recency window** | How many days counts as "recent" (default: 90) |

### 2. Link to Media Bar

In **Dashboard → Plugins → Media Bar**, set the **Avatars Playlist** field to the same name you configured above (default: `Featured Recommendations`).

### 3. Run the task

Go to **Dashboard → Scheduled Tasks → Media Bar Recommendations → Update Media Bar Recommendations** and click ▶ to run it immediately for the first time.

The task will then run automatically every day at 3 AM.

---

## Reporting Issues

Please open an issue on this repository for anything related to recommendation quality, plugin settings, or scheduled task behaviour.

For issues with the Media Bar visual component itself (layout, buttons, display), please report to [IAmParadox27/jellyfin-plugin-media-bar](https://github.com/IAmParadox27/jellyfin-plugin-media-bar/issues).

---

## Credits

This plugin was built as a companion to the original [Media Bar plugin](https://github.com/IAmParadox27/jellyfin-plugin-media-bar) by [@IAmParadox27](https://github.com/IAmParadox27), which itself builds on the work of [@MakD](https://github.com/MakD), [@BobHasNoSoul](https://github.com/BobHasNoSoul) and [@SethBacon](https://github.com/SethBacon).

---

## License

This project is licensed under the [MIT License](LICENSE).
